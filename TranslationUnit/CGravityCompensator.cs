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
            throw new NotImplementedException();
        }
    }
}