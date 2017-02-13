using System;

namespace TranslationUnit
{
    class CKinematicSensor : ISensor
    {
        private double[] _state;
        private ISensor _nextSensor;
        private IKalmanFilter[] _filterArray;

        internal CKinematicSensor( ISensor nextSensor )
        {
            _filterArray = new IKalmanFilter[] { new CKalmanFilter(), new CKalmanFilter(), new CKalmanFilter(), new CKalmanFilter(), new CKalmanFilter(), new CKalmanFilter() };
            _state = new double[] { 0, 0, 0, 0, 0, 0 };
            _nextSensor = nextSensor;
        }

        double[] ISensor.getValue( double[] input )
        {
            // Recompute and return data ready to be used.

            throw new NotImplementedException();
        }

        double[] ISensor.getState()
        {
            // Need to subtract the values from previous sensors; just don't actually recompute stuff.

            return _state;
        }

        private double[] _findRotation( double[] array )
        {
            throw new NotImplementedException();
        }
    }

    class MockKinematicSensor : ISensor
    {
        double[] ISensor.getState()
        {
            return new double[] { 0, 0, 0 };
        }

        double[] ISensor.getValue( double[] input )
        {
            return input;
        }
    }
}
