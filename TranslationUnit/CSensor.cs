using System;

namespace TranslationUnit
{
    abstract class CSensor
    {
        // Sensor parameters
        protected string _id;
        protected double _normalizer;
        protected double _deltaT;
        protected double _probabilityFactor;

        protected double _x_offset = 0;
        protected double _y_offset = 0;
        protected double _z_offset = 0;

        // --- Estimator variables --- 
        private static readonly double[] FIR_COEFF = { 0.2782, 0.4437, 0.2782 };

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

        protected CSensor( double k1, double k2 )
        {
            _xFilters_large = new IFilter[] { new CFIRFilter( FIR_COEFF ), new CKalmanFilter( k1, k2, _deltaT ) };
            _yFilters_large = new IFilter[] { new CFIRFilter( FIR_COEFF ), new CKalmanFilter( k1, k2, _deltaT ) };
            _zFilters_large = new IFilter[] { new CFIRFilter( FIR_COEFF ), new CKalmanFilter( k1, k2, _deltaT ) };

            _xFilters_small = new IFilter[] { new CFIRFilter( FIR_COEFF ), new CKalmanFilter( k1, k2, _deltaT ) };
            _yFilters_small = new IFilter[] { new CFIRFilter( FIR_COEFF ), new CKalmanFilter( k1, k2, _deltaT ) };
            _zFilters_small = new IFilter[] { new CFIRFilter( FIR_COEFF ), new CKalmanFilter( k1, k2, _deltaT ) };
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

        public void setOffsets (double x, double y, double z)
        {
            _x_offset = x;
            _y_offset = y;
            _z_offset = z;
        }

        protected abstract double estimate( ref double small, ref double large, IFilter[] filters_small, IFilter[] filters_large, int input );
    }
}