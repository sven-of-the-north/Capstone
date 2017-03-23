using System;

namespace TranslationUnit
{
    /// <summary>
    /// A 3-axis gyroscope
    /// </summary>
    class CGyroscope : CSensor
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"> Name </param>
        /// <param name="normalizer"> Normalization coefficient </param>
        /// <param name="nextSensor"> Next sensor (if any) </param>
        /// <param name="deltaT"> Time step (s) for integration (default: 60Hz) </param>
        internal CGyroscope( string id, double normalizer, CSensor nextSensor = null, double deltaT = 0.016667, double pF = 0.99 ) 
            : base()
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
    }

    class MockGyroscope : CSensor
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
