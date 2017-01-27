using System;
using System.Drawing.Drawing2D;
//using System.Windows.Media.Media3D;

namespace TranslationUnit
{
    class CKalmanFilter : IKalmanFilter
    {
        private struct KalmanState
        {
            /** source: http://interactive-matter.eu/blog/2009/12/18/filtering-sensor-data-with-a-kalman-filter/ **/

            public double _q;    // process noise covariance
            public double _r;    // measurement noise covariance
            public double _x;    // value
            public double _p;    // estimateion error covariance
            public double _k;    // kalman gain

            public KalmanState( double q = 0, double r = 0, double x = 0, double p = 0, double k = 0 )
            {
                _q = q;
                _r = r;
                _x = x;
                _p = p;
                _k = k;
            }
        };

        KalmanState _state;

        internal CKalmanFilter()
        {
            _state = new KalmanState();
        }

        double IKalmanFilter.update( double input )
        {
            //_state._p = _state._p + _state._q;

            //_state._k = _state._p / ( _state._p + _state._r );
            _state._x = _state._x + _state._k * ( input - _state._x );
            //_state._p = ( 1 - _state._k ) * _state._p;

            return _state._x;
        }
    }
}
