using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslationUnit
{
    /// <summary>
    /// Sensor IDs
    /// </summary>
    public sealed class eSensor
    {
        private readonly String _name;
        private readonly String _value;
        private readonly eSensorType _type;
        private static readonly Dictionary<string, eSensor> _instance = new Dictionary<string,eSensor>();

        public static readonly eSensor Hand_Accel    = new eSensor( "Hand_Accel" , "a0", eSensorType.Accelerometer );
        public static readonly eSensor Hand_Gyro     = new eSensor( "Hand_Gyro" , "g0", eSensorType.Gyroscope );
        public static readonly eSensor Thumb_Prox    = new eSensor( "Thumb_Prox" , "g1", eSensorType.Gyroscope );
        public static readonly eSensor Thumb_Dist    = new eSensor( "Thumb_Dist" , "g2", eSensorType.Gyroscope );
        public static readonly eSensor Index_Prox    = new eSensor( "Index_Prox" , "g3", eSensorType.Gyroscope );
        public static readonly eSensor Index_Dist    = new eSensor( "Index_Dist" , "g4", eSensorType.Gyroscope );
        public static readonly eSensor Middle_Prox   = new eSensor( "Middle_Prox" , "g5", eSensorType.Gyroscope );
        public static readonly eSensor Middle_Dist   = new eSensor( "Middle_Dist" , "g6", eSensorType.Gyroscope );
        public static readonly eSensor Thumb_Prox_a  = new eSensor( "Thumb_Prox_a" , "a1", eSensorType.Accelerometer );
        public static readonly eSensor Thumb_Dist_a  = new eSensor( "Thumb_Dist_a" , "a2", eSensorType.Accelerometer );
        public static readonly eSensor Index_Prox_a  = new eSensor( "Index_Prox_a" , "a3", eSensorType.Accelerometer );
        public static readonly eSensor Index_Dist_a  = new eSensor( "Index_Dist_a" , "a4", eSensorType.Accelerometer );
        public static readonly eSensor Middle_Prox_a = new eSensor( "Middle_Prox_a" , "a5", eSensorType.Accelerometer );
        public static readonly eSensor Middle_Dist_a = new eSensor( "Middle_Dist_a" , "a6", eSensorType.Accelerometer );

        private eSensor( string name, string value, eSensorType type )
        {
            _name = name;
            _value = value;
            _type = type;

            _instance[name] = this;
        }

        public override string ToString()
        {
            return _name;
        }

        public string value()
        { 
            return _value;
        }

        public eSensorType type()
        {
            return _type;
        }

        public static List<string> values()
        {
            return _instance.Keys.ToList<string>();
        }
        
        public static explicit operator eSensor( string str )
        {
            eSensor result;
            if ( _instance.TryGetValue( str, out result ) )
                return result;
            else
                throw new InvalidCastException();
        }
    };
}
