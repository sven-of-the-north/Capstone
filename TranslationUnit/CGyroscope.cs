using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslationUnit
{
    class CGyroscope : ISensor
    {
        private static readonly int FIR_ORDER_GYRO = 9;
        private static readonly double[] FIR_COEFF_GYRO = { -0.1896, 0.1332, -0.0877, 0.2782, 0.5444, 0.2782, -0.0877, 0.1332, -0.1896 };

        private static readonly double PROBABILITY_FACTOR = 0.99;

        // Sensor parameters
        private string _id;
        private double _normalizer;
        private ISensor _nextSensor;
        private double _deltaT;

        // Add filters to the filter arrays in order of operation; ie. _xFilter[0] will be used before _xFilter[1], etc.
        private IFilter[] _xFilters_large;
        private IFilter[] _yFilters_large;
        private IFilter[] _zFilters_large;

        private IFilter[] _xFilters_small;
        private IFilter[] _yFilters_small;
        private IFilter[] _zFilters_small;

        private double _x = Double.NegativeInfinity;
        private double _y = Double.NegativeInfinity;
        private double _z = Double.NegativeInfinity;

        private double _x_large = 0;
        private double _y_large = 0;
        private double _z_large = 0;

        private double _x_small = 0;
        private double _y_small = 0;
        private double _z_small = 0;

        internal CGyroscope( string id, double normalizer, ISensor nextSensor = null, double deltaT = ( double )1 / 60 )
        {
            _xFilters_large = new IFilter[] { new CFIRFilter( FIR_ORDER_GYRO, FIR_COEFF_GYRO ), new CKalmanFilter( 0.74020, 1, deltaT ) };
            _yFilters_large = new IFilter[] { new CFIRFilter( FIR_ORDER_GYRO, FIR_COEFF_GYRO ), new CKalmanFilter( 0.61470, 1, deltaT ) };
            _zFilters_large = new IFilter[] { new CFIRFilter( FIR_ORDER_GYRO, FIR_COEFF_GYRO ), new CKalmanFilter( 0.26480, 1, deltaT ) };

            _id = id;
            _nextSensor = nextSensor;
            _normalizer = normalizer;
            _deltaT = deltaT;
        }

        private double estimate( ref double small, ref double large, IFilter[] filters_small, IFilter[] filters_large, double input )
        {
            double new_large = 0;
            double new_small = 0;

            if ( ( Math.Abs(large) - Math.Abs(small) ) / 2 < Math.Abs(input) )
            {
                new_large = input;
                new_small = small;
            }
            else
            {
                new_large = large;
                new_small = input;
            }

            foreach ( IFilter filter in filters_large )
                new_large = filter.filter( new_large )[0];

            foreach ( IFilter filter in filters_small )
                new_small = filter.filter( new_small )[0];

            large = PROBABILITY_FACTOR * new_large + ( 1 - PROBABILITY_FACTOR ) * new_small;
            small = new_small;

            return new_large;
        }

        public double[] getValue( double[] input )
        {
            _x = estimate( ref _x_small, ref _x_large, _xFilters_small, _xFilters_large, input[0]);
            _y = estimate( ref _y_small, ref _y_large, _yFilters_small, _yFilters_large, input[1]);
            _z = estimate( ref _z_small, ref _z_large, _zFilters_small, _zFilters_large, input[2]);

            return new double[] { _x, _y, _z };
        }

        public double[] getState()
        {
            return new double[] { _x, _y, _z };
        }
    }
}
