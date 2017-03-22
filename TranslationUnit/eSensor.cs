using System;
using System.Collections.Generic;

namespace TranslationUnit
{
    /// <summary>
    /// Sensor IDs
    /// </summary>
    public sealed class eSensor
    {
        private readonly string _name;
        private static readonly Dictionary<string, eSensor> _name_to_value = new Dictionary<string,eSensor>();
        private static readonly Dictionary<eSensor, string> _value_to_name = new Dictionary<eSensor,string>();

        public static readonly eSensor Hand_Accel    = new eSensor( "a0" ); //0x10
        public static readonly eSensor Hand_Gyro     = new eSensor( "g0" ); //0x20
        public static readonly eSensor Thumb_Prox    = new eSensor( "g1" ); //0x30
        public static readonly eSensor Thumb_Dist    = new eSensor( "g2" ); //0x40
        public static readonly eSensor Index_Prox    = new eSensor( "g3" ); //0x50
        public static readonly eSensor Index_Dist    = new eSensor( "g4" ); //0x60
        public static readonly eSensor Middle_Prox   = new eSensor( "g5" ); //0x70
        public static readonly eSensor Middle_Dist   = new eSensor( "g6" ); //0x80
/*      public static readonly eSensor Thumb_Prox_a  = new eSensor( "a1" );
        public static readonly eSensor Thumb_Dist_a  = new eSensor( "a2" );
        public static readonly eSensor Index_Prox_a  = new eSensor( "a3" );
        public static readonly eSensor Index_Dist_a  = new eSensor( "a4" );
        public static readonly eSensor Middle_Prox_a = new eSensor( "a5" );
        public static readonly eSensor Middle_Dist_a = new eSensor( "a6" );*/
        public static readonly eSensor Invalid       = new eSensor( "NA" ); //0xFF

        private eSensor( string name )
        {
            _name = name;

            if ( name == "NA" )
                return;

            _name_to_value[name] = this;
            _value_to_name[this] = name;
        }

        public override string ToString()
        {
            return _name;
        }

        public eSensorType type()
        {
            switch ( _name[0] )
            {
                case 'a':
                    return eSensorType.Accelerometer;
                case 'g':
                    return eSensorType.Gyroscope;
                default:
                    throw new NotImplementedException();
            }
        }

        public static List<string> values()
        {
            return new List<string>( _name_to_value.Keys );
        }
        
        public static explicit operator eSensor( string str )
        {
            eSensor result;
            if ( _name_to_value.TryGetValue( str, out result ) )
                return result;
            else
                throw new InvalidCastException();
        }

        public static explicit operator eSensor( byte val )
        {
            switch ( val )
            {
                case ( 16 ):
                    return eSensor.Hand_Accel;
                case ( 32 ):
                    return eSensor.Hand_Gyro;
                case ( 48 ):
                    return eSensor.Thumb_Prox;
                case ( 64 ):
                    return eSensor.Thumb_Dist;
                case ( 80 ):
                    return eSensor.Index_Prox;
                case ( 96 ):
                    return eSensor.Index_Dist;
                case ( 112 ):
                    return eSensor.Middle_Prox;
                case ( 128 ):
                    return eSensor.Middle_Dist;
                default:
                    throw new InvalidCastException();
            }
        }

        public static explicit operator string( eSensor sensor )
        {
            string result;
            if ( _value_to_name.TryGetValue( sensor, out result ) )
                return result;
            else
                throw new InvalidCastException();
        }
    };
}
