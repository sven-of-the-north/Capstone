using System;
//using System.Windows.Media.Media3D;

namespace TranslationUnit
{
    class CKinematicSensor : ISensor
    {
        private double[] _prevState;
        private ISensor _nextSensor;
        private IKalmanFilter[] _filterArray;

        internal CKinematicSensor( ISensor nextSensor )
        {
            _filterArray = new IKalmanFilter[] { new CKalmanFilter(), new CKalmanFilter(), new CKalmanFilter(), new CKalmanFilter(), new CKalmanFilter(), new CKalmanFilter() };
            _prevState = new double[] { 0, 0, 0 };
            _nextSensor = nextSensor;
        }

        double[] ISensor.getValue()
        {
            throw new NotImplementedException();
        }

        private double[] _findRotation( double[] array )
        {
            throw new NotImplementedException();
        }
    }
}
