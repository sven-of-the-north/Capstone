using System;

namespace TranslationUnit
{
    /// <summary>
    /// A 3-axis gyroscope
    /// </summary>
    class CGyroscope : ISensor
    {
        private static readonly double[] FIR_COEFF_GYRO = { -0.1896, 0.1332, -0.0877, 0.2782, 0.5444, 0.2782, -0.0877, 0.1332, -0.1896 };
        private static readonly double KALMAN_GAIN_1 = 0.2138;
        private static readonly double KALMAN_GAIN_2 = 1.5357;

        private static readonly double PROBABILITY_FACTOR = 0.99;

        // Sensor parameters
        private string _id;
        private double _normalizer;
        private ISensor _nextSensor;
        private double _deltaT;

        // Add filters to the filter arrays in order of operation; ie. _xFilter[0] will be used before _xFilter[1], etc.
        private IFilter[] _xFilters_large;
        private IFilter[] _yFilters_large;
        private IFilter[] _zFilters_large;

        private IFilter[] _xFilters_small;
        private IFilter[] _yFilters_small;
        private IFilter[] _zFilters_small;

        private double _x = double.NegativeInfinity;
        private double _y = double.NegativeInfinity;
        private double _z = double.NegativeInfinity;

        private double _x_large = 0;
        private double _y_large = 0;
        private double _z_large = 0;

        private double _x_small = 0;
        private double _y_small = 0;
        private double _z_small = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"> Name </param>
        /// <param name="normalizer"> Normalization coefficient </param>
        /// <param name="nextSensor"> Next sensor (if any) </param>
        /// <param name="deltaT"> Time step (s) for integration (default: 60Hz) </param>
        internal CGyroscope( string id, double normalizer, ISensor nextSensor = null, double deltaT = ( double )1 / 60 )
        {
            _xFilters_large = new IFilter[] { new CFIRFilter( FIR_COEFF_GYRO ), new CKalmanFilter( KALMAN_GAIN_1, KALMAN_GAIN_2, deltaT ) };
            _yFilters_large = new IFilter[] { new CFIRFilter( FIR_COEFF_GYRO ), new CKalmanFilter( KALMAN_GAIN_1, KALMAN_GAIN_2, deltaT ) };
            _zFilters_large = new IFilter[] { new CFIRFilter( FIR_COEFF_GYRO ), new CKalmanFilter( KALMAN_GAIN_1, KALMAN_GAIN_2, deltaT ) };
                                                                                                   
            _xFilters_small = new IFilter[] { new CFIRFilter( FIR_COEFF_GYRO ), new CKalmanFilter( KALMAN_GAIN_1, KALMAN_GAIN_2, deltaT ) };
            _yFilters_small = new IFilter[] { new CFIRFilter( FIR_COEFF_GYRO ), new CKalmanFilter( KALMAN_GAIN_1, KALMAN_GAIN_2, deltaT ) };
            _zFilters_small = new IFilter[] { new CFIRFilter( FIR_COEFF_GYRO ), new CKalmanFilter( KALMAN_GAIN_1, KALMAN_GAIN_2, deltaT ) };

            _id = id;
            _nextSensor = nextSensor;
            _normalizer = normalizer;
            _deltaT = deltaT;
        }

        /// <summary>
        /// The gyroscope signal flips back and forth between two distinct values, the larger of which is typically correct. Therefore it is necessary
        /// that we treat each value as a separate signal.
        /// </summary>
        private double estimate( ref double small, ref double large, IFilter[] filters_small, IFilter[] filters_large, int input )
        {
            double x_large = 0;
            double x_small = 0;
            double dx_large = 0;
            double dx_small = 0;

            double[] xVec = new double[2] { 0, 0 };

            if ( ( Math.Abs(large) - Math.Abs(small) ) / 2 < Math.Abs(input) )
            {
                x_large = input;
                x_small = small;
            }
            else
            {
                x_large = large;
                x_small = input;
            }

            foreach ( IFilter filter in filters_large )
            {
                xVec = filter.filter( x_large );
                x_large = xVec[0];
                dx_large = xVec[1];
            }

            foreach ( IFilter filter in filters_small )
            {
                xVec = filter.filter( x_small );
                x_small = xVec[0];
                dx_small = xVec[1];
            }

            large = PROBABILITY_FACTOR * ( x_large + _deltaT * dx_large ) + ( 1 - PROBABILITY_FACTOR ) * ( x_small + _deltaT * dx_small );
            small = x_small + _deltaT * dx_small;

            return x_large;
        }

        /// <summary>
        /// All input values are run through their own set of stateful filters that can be defined in the constructor for this class.
        /// </summary>
        public double[] getValue( int[] input )
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
        public double[] getState()
        {
            return new double[] { _x, _y, _z };
        }
    }

    class MockGyroscope : ISensor
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
