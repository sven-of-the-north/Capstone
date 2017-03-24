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
        //private static readonly double[] FIR_COEFF_ACCEL =
        //    { -0.1057, -0.1098, -0.1127, -0.1144, 0.8850, -0.1144, -0.1127, -0.1098, -0.1057 };

        private double[] FIR_COEFF_ACCEL;
        private static readonly int FIR_ORDER = 30;
        private static readonly int DEADZONE = 2400;

        private static readonly double KALMAN_GAIN_1 = 0.2777;
        private static readonly double KALMAN_GAIN_2 = 2.6875;

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
        internal CAccelerometer( string id, double normalizer, CSensor nextSensor = null, double deltaT = 0.016667, double pF = 0.9 ) 
            : base( KALMAN_GAIN_1 , KALMAN_GAIN_2 )
        {
            _id = id;
            _nextSensor = nextSensor;
            _normalizer = normalizer;
            _probabilityFactor = pF;
            _deltaT = deltaT;

            generateFIRcoefficients();

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
            double x = estimate( ref _x_small /*unused*/, ref _x_large, _xFilters_small /*unused*/, _xFilters_large, input[0] );
            double y = estimate( ref _y_small /*unused*/, ref _y_large, _yFilters_small /*unused*/, _yFilters_large, input[1] );
            double z = estimate( ref _z_small /*unused*/, ref _z_large, _zFilters_small /*unused*/, _zFilters_large, input[2] );

            /*
            for ( int i = 0; i < _xFilters.Length; ++i )
                _x -= _xFilters[i].filter( _x )[0];

            for ( int i = 0; i < _yFilters.Length; ++i )
                _y -= _yFilters[i].filter( _y )[0];

            for ( int i = 0; i < _zFilters.Length; ++i )
                _z -= _zFilters[i].filter( _z )[0];
            */

            _x = _integratorX.integrate( x ) * _normalizer;
            _y = _integratorY.integrate( y ) * _normalizer;
            _z = _integratorZ.integrate( z ) * _normalizer;

            //_x -= 0.0061;
            //_y += 0.3027;
            //_z += 0.3027;

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

        protected override double estimate( ref double small, ref double large, IFilter[] filters_small, IFilter[] filters_large, int input )
        {
            double x_large = 0;
            double dx_large = 0;

            double[] xVec = new double[2] { 0, 0 };

            if ( DEADZONE < Math.Abs( input ) )
                x_large = input;
            else
                x_large = large;

            for ( int i = 0; i < filters_large.Length; ++i )
            {
                xVec = filters_large[i].filter( x_large );
                x_large = xVec[0];
                dx_large = xVec[1];
            }

            large = _probabilityFactor * ( x_large + _deltaT * dx_large );

            return x_large;
        }

        private void generateFIRcoefficients()
        {
            FIR_COEFF_ACCEL = new double[FIR_ORDER];
            double sum = 0;

            for ( int i = FIR_ORDER; i > 0; --i )
            {
                double coeff = Math.Log( i );
                FIR_COEFF_ACCEL[FIR_ORDER - i] = coeff;
                sum += coeff;
            }

            for ( int i = 0; i < FIR_ORDER; ++i )
                FIR_COEFF_ACCEL[i] /= sum;
        }
    }

    class MockAccelerometer : CSensor
    {
        MockAccelerometer() : base( 0, 0 ) { }

        public override double[] getState()
        {
            return new double[] { 0, 0, 0 };
        }

        public override double[] getValue( int[] input )
        {
            return new double[] { input[0], input[1], input[2] };
        }

        protected override double estimate( ref double small, ref double large, IFilter[] filters_small, IFilter[] filters_large, int input )
        {
            throw new NotImplementedException();
        }
    }
}
