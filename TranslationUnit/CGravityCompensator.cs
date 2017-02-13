using System;

namespace TranslationUnit
{
    internal class CGravityCompensator
    {
        private double[] _prevState;

        internal CGravityCompensator()
        {
            _prevState = new double[] { 0, 0, 0 };
        }

        internal double[] compensate( double[] input )
        {
            return new double[] { input[0] - _prevState[0], input[1] - _prevState[1], input[2] - _prevState[2] };
        }
    }
}