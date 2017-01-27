using System;

namespace TranslationUnit
{
    class CPressureSensor : ISensor
    {
        private IKalmanFilter[] _filterArray;

        internal CPressureSensor()
        {
            _filterArray = new IKalmanFilter[] { new CKalmanFilter(), new CKalmanFilter(), new CKalmanFilter() };
        }

        double[] ISensor.getValue( double[] input )
        {
            throw new NotImplementedException();
        }

        double[] ISensor.getState()
        {
            throw new NotImplementedException();
        }
    }
}
