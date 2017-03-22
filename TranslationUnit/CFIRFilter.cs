using System;

namespace TranslationUnit
{
    /// <summary>
    /// A direct form finite impulse response filter
    /// </summary>
    class CFIRFilter : IFilter
    {
        private double _sum;    // previous output
        private int _order = 0; // order of this filter
        double[] _coefficients; // computation coefficients
        double[] _inputs;       // previous inputs

        /// <summary>
        /// Constructor for FIR filter
        /// </summary>
        /// <param name="order"> Order of this filter </param>
        /// <param name="coefficients"> Coefficients for computation </param>
        internal CFIRFilter( int order, double[] coefficients )
        {
            _sum = 0;
            _order = order;
            _coefficients = coefficients;
            _inputs = new double[order];
        }

        /// <summary>
        /// Since this is a direct form FIR filter, each output is the weighted sum of a certain number of previous inputs 
        /// </summary>
        public double[] filter( double input )
        {
            _sum = 0;

            Array.Copy( _inputs, 0, _inputs, 1, _order - 1 );
            _inputs[0] = input;

            for ( int i = 0; i < _order; ++i )
                _sum += _inputs[i] * _coefficients[i];

            return new double[] { _sum, 0 };
        }

        /// <summary>
        /// Does no computation, simply returns the last calculated output value
        /// </summary>
        public double[] getState()
        {
            return new double[] { _sum, 0 };
        }
    }

    class MockFIRFilter : IFilter
    {
        public double[] filter( double input )
        {
            return new double[] { input, 0 };
        }

        public double[] getState()
        {
            return new double[] { 0, 0 };
        }
    }
}
