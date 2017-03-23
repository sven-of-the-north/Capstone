using System;

namespace TranslationUnit
{
    abstract class CSensor
    {
        // Sensor parameters
        protected string _id;
        protected double _normalizer;
        protected CSensor _nextSensor;
        protected double _deltaT;
        protected double _probabilityFactor;

        // --- Estimator variables --- 
        private static readonly double[] FIR_COEFF = { 0.2782, 0.4437, 0.2782 };
        private static readonly double KALMAN_GAIN_1 = 0.1670;
        private static readonly double KALMAN_GAIN_2 = 0.9127;

        protected double _x_large = 0;
        protected double _y_large = 0;
        protected double _z_large = 0;

        protected double _x_small = 0;
        protected double _y_small = 0;
        protected double _z_small = 0;

        // Add filters to the filter arrays in order of operation; ie. _xFilter[0] will be used before _xFilter[1], etc.
        protected IFilter[] _xFilters_large;
        protected IFilter[] _yFilters_large;
        protected IFilter[] _zFilters_large;

        protected IFilter[] _xFilters_small;
        protected IFilter[] _yFilters_small;
        protected IFilter[] _zFilters_small;

        protected double _x = Double.NegativeInfinity;
        protected double _y = Double.NegativeInfinity;
        protected double _z = Double.NegativeInfinity;

        protected CSensor()
        {
            _xFilters_large = new IFilter[] { new CFIRFilter( FIR_COEFF ), new CKalmanFilter( KALMAN_GAIN_1, KALMAN_GAIN_2, _deltaT ) };
            _yFilters_large = new IFilter[] { new CFIRFilter( FIR_COEFF ), new CKalmanFilter( KALMAN_GAIN_1, KALMAN_GAIN_2, _deltaT ) };
            _zFilters_large = new IFilter[] { new CFIRFilter( FIR_COEFF ), new CKalmanFilter( KALMAN_GAIN_1, KALMAN_GAIN_2, _deltaT ) };

            _xFilters_small = new IFilter[] { new CFIRFilter( FIR_COEFF ), new CKalmanFilter( KALMAN_GAIN_1, KALMAN_GAIN_2, _deltaT ) };
            _yFilters_small = new IFilter[] { new CFIRFilter( FIR_COEFF ), new CKalmanFilter( KALMAN_GAIN_1, KALMAN_GAIN_2, _deltaT ) };
            _zFilters_small = new IFilter[] { new CFIRFilter( FIR_COEFF ), new CKalmanFilter( KALMAN_GAIN_1, KALMAN_GAIN_2, _deltaT ) };
        }

        /// <summary>
        /// All input values are run through their own set of stateful filters that can be defined in the constructor for this class
        /// </summary>
        /// <param name="input"> Raw [x, y, z] values </param>
        /// <returns> Calculated [x, y, z] values </returns>
        public abstract double[] getValue( int[] input );

        /// <summary>
        /// Does no computation, simply returns the last calculated output values
        /// </summary>
        /// <returns> Last calculated output values [x, y, z] </returns>
        public abstract double[] getState();

        protected double estimate( ref double small, ref double large, IFilter[] filters_small, IFilter[] filters_large, int input )
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
}