using System;
using System.Drawing.Drawing2D;
//using System.Windows.Media.Media3D;

namespace TranslationUnit
{
    class CKalmanFilter : IKalmanFilter
    {
        public Matrix _gain;
        public Matrix _delay;
        public Matrix _measurementModel;
        public Matrix _linearModel;
        public double[] _prevState;

        internal CKalmanFilter()
        {
            _gain = new Matrix();
            _delay = new Matrix();
            _measurementModel = new Matrix();
            _linearModel = new Matrix();
            _prevState = new double[] { 0, 0, 0 };
        }

        double[] IKalmanFilter.filter( double[] array )
        {
            throw new NotImplementedException();
        }
    }
}
