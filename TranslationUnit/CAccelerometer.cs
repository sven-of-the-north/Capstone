using System;

namespace TranslationUnit
{
    struct Integrator
    {
        double _velocity;
        double _prevAccel;
        double _deltaT;

        internal Integrator( double deltaT )
        {
            _velocity = 0;
            _prevAccel = 0;
            _deltaT = deltaT;
        }

        internal double integrate( double input )
        {
            _velocity = ( ( input - _prevAccel ) / 2 + _prevAccel ) * _deltaT + _velocity;

            _prevAccel = input;

            return _velocity;
        }
    }

    /// <summary>
    /// A 3-axis accelerometer
    /// </summary>
    class CAccelerometer : ISensor
    {
        private static readonly double[] FIR_COEFF_ACCEL = { -0.0210, -0.0160, -0.0206, -0.0256, -0.0301, -0.0341, -0.0371, -0.0391,  0.9603, -0.0391, -0.0371, -0.0341, -0.0301   ,-0.0256   ,-0.0206   ,-0.0160   ,-0.0210 };
        private static readonly double KALMAN_GAIN = 0.5;

        private static readonly double[] FIR_COEFF_ACCEL_POST = { -0.1896, 0.1332, -0.0877, 0.2782, 0.5444, 0.2782, -0.0877, 0.1332, -0.1896 };

        // Used for integration of accelerometer values
        private Integrator _integrator;

        // Sensor parameters
        private string _id;
        private double _normalizer;
        private ISensor _nextSensor;
        private double _deltaT;

        // Add filters to the filter arrays in order of operation; ie. _xFilter[0] will be used before _xFilter[1], etc.
        private IFilter[] _xFilters;
        private IFilter[] _yFilters;
        private IFilter[] _zFilters;

        private IFilter[] _xFilters_post;
        private IFilter[] _yFilters_post;
        private IFilter[] _zFilters_post;

        private double _x = Double.NegativeInfinity;
        private double _y = Double.NegativeInfinity;
        private double _z = Double.NegativeInfinity;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"> Name </param>
        /// <param name="normalizer"> Normalization coefficient </param>
        /// <param name="nextSensor"> Next sensor (if any) </param>
        /// <param name="deltaT"> Time step (s) for integration (default: 60Hz) </param>
        internal CAccelerometer( string id, double normalizer, ISensor nextSensor = null, double deltaT = (double)1/60 )
        {
            _xFilters = new IFilter[] { new CFIRFilter( FIR_COEFF_ACCEL ), new CKalmanFilter( KALMAN_GAIN ) };
            _yFilters = new IFilter[] { new CFIRFilter( FIR_COEFF_ACCEL ), new CKalmanFilter( KALMAN_GAIN ) };
            _zFilters = new IFilter[] { new CFIRFilter( FIR_COEFF_ACCEL ), new CKalmanFilter( KALMAN_GAIN ) };

            _xFilters_post = new IFilter[] { new CFIRFilter( FIR_COEFF_ACCEL_POST ) };
            _yFilters_post = new IFilter[] { new CFIRFilter( FIR_COEFF_ACCEL_POST ) };
            _zFilters_post = new IFilter[] { new CFIRFilter( FIR_COEFF_ACCEL_POST ) };

            _integrator = new Integrator();

            _id = id;
            _nextSensor = nextSensor;
            _normalizer = normalizer;
            _deltaT = deltaT;

            _integrator = new Integrator( deltaT );
        }

        /// <summary>
        /// All input values are run through their own set of stateful filters that can be defined in the constructor for this class.
        /// Integration is performed to produce instantaneous velocity.
        /// </summary>
        public double[] getValue( int[] input )
        {
            double x = input[0];
            double y = input[1];
            double z = input[2];

            try
            {
                for ( int i = 0; i < _xFilters.Length; ++i )
                    x = _xFilters[i].filter( x )[0];

                for ( int i = 0; i < _yFilters.Length; ++i )
                    y = _yFilters[i].filter( y )[0];

                for ( int i = 0; i < _zFilters.Length; ++i )
                    z = _zFilters[i].filter( z )[0];

                x = _integrator.integrate( x );
                y = _integrator.integrate( y );
                z = _integrator.integrate( z );

                for ( int i = 0; i < _xFilters_post.Length; ++i )
                    x = _xFilters_post[i].filter( x )[0];

                for ( int i = 0; i < _yFilters_post.Length; ++i )
                    y = _yFilters_post[i].filter( y )[0];

                for ( int i = 0; i < _zFilters_post.Length; ++i )
                    z = _zFilters_post[i].filter( z )[0];
            }
            catch (Exception e)
            {
                throw e;
            }

            _x = x * _normalizer;
            _y = y * _normalizer;
            _z = z * _normalizer;

            return new double[] { _x, _y, _z };
        }

        /// <summary>
        /// Does no computation, simply returns the last calculated output values
        /// </summary>
        public double[] getState()
        {
            // May need to subtract the values from previous sensors; just don't actually recompute stuff.

            return new double[] { _x, _y, _z };
        }
    }

    class MockAccelerometer : ISensor
    {
        public double[] getState()
        {
            return new double[] { 0, 0, 0 };
        }

        public double[] getValue( int[] input )
        {
            return new double[] { input[0], input[1], input[2] };
        }
    }
}
