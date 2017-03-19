using System;
using System.Drawing.Drawing2D;
//using System.Windows.Media.Media3D;

namespace TranslationUnit
{
    /// <summary>
    /// A basic Kalman Filter
    /// </summary>
    class CKalmanFilter : IFilter
    {
        double _x;      // value
        double _dx;     // derivative
        double _k1;     // kalman gain for value
        double _k2;     // kalman gain for derivative
        double _dt;     // time step

        /// <summary>
        /// Constructor for Kalman filter
        /// </summary>
        /// <param name="gain"> Kalman gain </param>
        /// <param name="initState"> Initial value (default: 0) </param>
        internal CKalmanFilter ( double k1, double k2 = 0, double dt = 0 )
        {
            _x = 0;
            _dx = 0;

            _k1 = k1;
            _k2 = k2;
            _dt = dt;
        }

        /// <summary>
        /// Each output is the sum of the last input and a weighted difference between the last input and the current input
        /// </summary>
        public double[] filter( double input )
        {
            double x = _x;
            double dx = _dx;

            _x = x + _dt * dx + _k1 * ( input - x + _dt * dx );
            _dx = dx + _k2 * ( input - x + _dt * dx );

            return new double[] { _x, _dx };
        }

        /// <summary>
        /// Does no computation, simply returns the last calculated output value
        /// </summary>
        public double[] getState()
        {
            return new double[] { _x, _dx };
        }
    }

    class MockKalmanFilter : IFilter
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
