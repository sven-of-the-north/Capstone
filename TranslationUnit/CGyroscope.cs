using System;

namespace TranslationUnit
{
    /// <summary>
    /// A 3-axis gyroscope
    /// </summary>
    class CGyroscope : CSensor
    {
        private static readonly double KALMAN_GAIN_1 = 0.1670;
        private static readonly double KALMAN_GAIN_2 = 0.9127;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"> Name </param>
        /// <param name="normalizer"> Normalization coefficient </param>
        /// <param name="nextSensor"> Next sensor (if any) </param>
        /// <param name="deltaT"> Time step (s) for integration (default: 60Hz) </param>
        internal CGyroscope( string id, double normalizer, CSensor nextSensor = null, double deltaT = 0.016667, double pF = 0.99 ) 
            : base( KALMAN_GAIN_1, KALMAN_GAIN_2 )
        {
            _id = id;
            _nextSensor = nextSensor;
            _normalizer = normalizer;
            _probabilityFactor = pF;
            _deltaT = deltaT;
        }

        /// <summary>
        /// All input values are run through their own set of stateful filters that can be defined in the constructor for this class.
        /// </summary>
        public override double[] getValue( int[] input )
        {
            _x = estimate( ref _x_small, ref _x_large, _xFilters_small, _xFilters_large, input[0]) * _normalizer + 180;
            _y = estimate( ref _y_small, ref _y_large, _yFilters_small, _yFilters_large, input[1]) * _normalizer + 180;
            _z = estimate( ref _z_small, ref _z_large, _zFilters_small, _zFilters_large, input[2]) * _normalizer + 180;

            if ( _x > 360 )
                _x = 360;
            if ( _y > 360 )
                _y = 360;
            if ( _z > 360 )
                _z = 360;

            if ( _x < 0 )
                _x = 0;
            if ( _y < 0 )
                _y = 0;
            if ( _z < 0 )
                _z = 0;

            return new double[] { _x, _y, _z };
        }

        /// <summary>
        /// Does no computation, simply returns the last calculated output values
        /// </summary>
        public override double[] getState()
        {
            return new double[] { _x, _y, _z };
        }

        protected override double estimate( ref double small, ref double large, IFilter[] filters_small, IFilter[] filters_large, int input )
        {
            double x_large = 0;
            double x_small = 0;
            double dx_large = 0;
            double dx_small = 0;

            double[] xVec = new double[2] { 0, 0 };

            if ( ( Math.Abs( large ) - Math.Abs( small ) ) / 2 < Math.Abs( input ) )
            {
                x_large = input;
                x_small = small;
            }
            else
            {
                x_large = large;
                x_small = input;
            }

            for ( int i = 0; i < filters_large.Length; ++i )
            {
                xVec = filters_large[i].filter( x_large );
                x_large = xVec[0];
                dx_large = xVec[1];
            }

            for ( int i = 0; i < filters_small.Length; ++i )
            {
                xVec = filters_small[i].filter( x_small );
                x_small = xVec[0];
                dx_small = xVec[1];
            }

            large = _probabilityFactor * ( x_large + _deltaT * dx_large ) + ( 1 - _probabilityFactor ) * ( x_small + _deltaT * dx_small );
            small = x_small + _deltaT * dx_small;

            return x_large;
        }
    }

    class MockGyroscope : CSensor
    {
        MockGyroscope() : base( 0, 0 ) { }

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
