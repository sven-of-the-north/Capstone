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
    class CAccelerometer : CSensor
    {
        private static readonly double[] FIR_COEFF_ACCEL =
            { -0.0210, -0.0160, -0.0206, -0.0256, -0.0301, -0.0341, -0.0371, -0.0391, 0.9603, -0.0391, -0.0371, -0.0341, -0.0301, -0.0256, -0.0206, -0.0160, -0.0210 }; 

        // Used for integration of accelerometer values
        private Integrator _integratorX;
        private Integrator _integratorY;
        private Integrator _integratorZ;

        private IFilter[] _xFilters;
        private IFilter[] _yFilters;
        private IFilter[] _zFilters;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"> Name </param>
        /// <param name="normalizer"> Normalization coefficient </param>
        /// <param name="nextSensor"> Next sensor (if any) </param>
        /// <param name="deltaT"> Time step (s) for integration (default: 60Hz) </param>
        internal CAccelerometer( string id, double normalizer, CSensor nextSensor = null, double deltaT = 0.016667, double pF = 0.99 ) 
            : base()
        {
            _id = id;
            _nextSensor = nextSensor;
            _normalizer = normalizer;
            _probabilityFactor = pF;
            _deltaT = deltaT;

            _xFilters = new IFilter[] { new CFIRFilter( FIR_COEFF_ACCEL ) };
            _yFilters = new IFilter[] { new CFIRFilter( FIR_COEFF_ACCEL ) };
            _zFilters = new IFilter[] { new CFIRFilter( FIR_COEFF_ACCEL ) };

            _integratorX = new Integrator( deltaT );
            _integratorY = new Integrator( deltaT );
            _integratorZ = new Integrator( deltaT );
        }

        /// <summary>
        /// All input values are run through their own set of stateful filters that can be defined in the constructor for this class.
        /// Integration is performed to produce instantaneous velocity.
        /// </summary>
        public override double[] getValue( int[] input )
        {
            double x = estimate( ref _x_small, ref _x_large, _xFilters_small, _xFilters_large, input[0] );
            double y = estimate( ref _y_small, ref _y_large, _yFilters_small, _yFilters_large, input[1] );
            double z = estimate( ref _z_small, ref _z_large, _zFilters_small, _zFilters_large, input[2] );

            try
            {
                for ( int i = 0; i < _xFilters.Length; ++i )
                    x = _xFilters[i].filter( x )[0];

                for ( int i = 0; i < _yFilters.Length; ++i )
                    y = _yFilters[i].filter( y )[0];

                for ( int i = 0; i < _zFilters.Length; ++i )
                    z = _zFilters[i].filter( z )[0];

                x = _integratorX.integrate( x );
                y = _integratorY.integrate( y );
                z = _integratorZ.integrate( z );
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
        public override double[] getState()
        {
            // May need to subtract the values from previous sensors; just don't actually recompute stuff.

            return new double[] { _x, _y, _z };
        }
    }

    class MockAccelerometer : CSensor
    {
        public override double[] getState()
        {
            return new double[] { 0, 0, 0 };
        }

        public override double[] getValue( int[] input )
        {
            return new double[] { input[0], input[1], input[2] };
        }
    }
}
