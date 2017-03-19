using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

namespace TranslationUnit
{
    /// <summary>
    /// Motor bitmasks
    /// </summary>
    public enum eMotors
    {
        none = 0,
        Motor7 = 2,
        Motor6 = 4,
        Motor5 = 8,
        Motor4 = 16,
        Motor3 = 32,
        Motor2 = 64,
        Motor1 = 128  
    };

    /// <summary>
    /// Sensor types
    /// </summary>
    public enum eSensorType
    {
        Accelerometer = 0,
        Gyroscope
    };

    /// <summary>
    /// Exception thrown if the serial port specified could not be initialized/opened
    /// </summary>
    [Serializable]
    public class InvalidSerialPortException : Exception
    {
        public InvalidSerialPortException() { }
        public InvalidSerialPortException( string message ) : base( message ) { }
        public InvalidSerialPortException( string message, Exception inner ) : base( message, inner ) { }
        protected InvalidSerialPortException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context ) : base( info, context ) { }
    }

    /// <summary>
    /// Main object used to represent the state of the Force Feedback Glove
    /// </summary>
    public class CTranslationUnit : ITranslationUnit
    {
        private const double NORMALIZE_ACCEL = 0.000061;
        private const double NORMALIZE_GYRO = 0.00875;

        private const int BAUDRATE = 115200;
        private const int DATABITS = 8;
        private const Handshake HANDSHAKE = Handshake.None;
        private const Parity PARITY = Parity.None;
        private const StopBits STOPBITS = StopBits.One;

        private Dictionary<eSensor, double[]> _valueMap;
        private Dictionary<eSensor, ISensor> _sensorMap;
        private SerialPort _serialPort;
        private CGravityCompensator _gravityCompensator;

        /// <summary>
        /// Constructor for a Translation Unit object
        /// </summary>
        /// <param name="portName"> Serial port name </param>
        /// <param name="baudRate"> Serial port baud rate (default 115200) </param>
        /// <param name="dataBits"> Serial port data bits (default 8) </param>
        /// <param name="handshake"> Serial port handshaking value (default None) </param>
        /// <param name="parity"> Serial port parity value (default None) </param>
        /// <param name="stopBits"> Serial port number of stopbits per byte (default One) </param>
        public CTranslationUnit( string portName )
        {
            if ( !_initialize( portName ) )
                throw new InvalidSerialPortException( "Could not open serial port for communication." );
        }

        /// <summary>
        /// Translation Unit destructor
        /// </summary>
        ~CTranslationUnit()
        {
            _sensorMap.Clear();
            _valueMap.Clear();
            _serialPort.Dispose();
        }

        /// <summary>
        /// Gets the latest reading from the sensor specified by SensorID
        /// </summary>
        /// <param name="sensorID"> Sensor to read from </param>
        /// <param name="x"> ignored </param>
        /// <param name="y"> ignored </param>
        /// <param name="z"> ignored </param>
        /// <returns> [x, y, z] values read from the specified sensor </returns>
        double[] ITranslationUnit.readSensor( eSensor sensorID, double x, double y, double z )
        {
            return _valueMap[sensorID];
        }

        /// <summary>
        /// Activates the brake specified by brakeID
        /// </summary>
        /// <param name="brakeID"> Brake to activate </param>
        /// <param name="brakeValue"> ignored (always 1) </param>
        void ITranslationUnit.applyBrake( int brakeID, double brakeValue )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// (Re-)Initializes all parameters and data fields within the Translation Unit
        /// </summary>
        /// <param name="portName"> Serial port name </param>
        /// <param name="baudRate"> Serial port baud rate (default 115200) </param>
        /// <param name="dataBits"> Serial port data bits (default 8) </param>
        /// <param name="handshake"> Serial port handshaking value (default None) </param>
        /// <param name="parity"> Serial port parity value (default None) </param>
        /// <param name="stopBits"> Serial port number of stopbits per byte (default One) </param>
        /// <returns> Initialization successful or not </returns>
        bool ITranslationUnit.initialize( string portName )
        {
            return _initialize( portName );
        }

        /// <summary>
        /// Handler for receiving serial data objects
        /// </summary>
        /// <param name="sender"> ignored </param>
        /// <param name="e"> ignored </param>
        void _serialPort_DataReceivedHandler( object sender, SerialDataReceivedEventArgs e )
        {
            string rawData = "";
            try
            {
                rawData = _serialPort.ReadLine();
            }
            catch ( Exception except )
            {
                Console.WriteLine( except.StackTrace );
                return;
            }

            string[] data = rawData.Split(',');

            eSensor sensor = (eSensor) data[0];

            if ( ( data.Length == 4 ) && _sensorMap.ContainsKey( sensor ) )
            {
                data[3] = data[3].Remove( data[3].Length - 1, 1 ); // strip newline character

                double raw_x = Convert.ToDouble(data[1]);
                double raw_y = Convert.ToDouble(data[2]);
                double raw_z = Convert.ToDouble(data[3]);

                try
                {
                    _valueMap[sensor] = _sensorMap[sensor].getValue( new double[] { raw_x, raw_y, raw_z } );
                }
                catch (Exception except)
                {
                    Console.WriteLine( except.StackTrace );
                    return;
                }
            }
        }

        /// <summary>
        /// (Re-)Initializes all parameters and data fields within the Translation Unit
        /// </summary>
        /// <param name="portName"> Serial port name </param>
        /// <param name="baudRate"> Serial port baud rate (default 115200) </param>
        /// <param name="dataBits"> Serial port data bits (default 8) </param>
        /// <param name="handshake"> Serial port handshaking value (default None) </param>
        /// <param name="parity"> Serial port parity value (default None) </param>
        /// <param name="stopBits"> Serial port number of stopbits per byte (default One) </param>
        /// <returns> Initialization successful or not </returns>
        private bool _initialize( string portName )
        {
            _initializeSensors();
            _gravityCompensator = new CGravityCompensator();

            try
            {
                _serialPort = new SerialPort( portName );
                _serialPort.BaudRate = BAUDRATE;
                _serialPort.DataBits = DATABITS;
                _serialPort.Handshake = HANDSHAKE;
                _serialPort.Parity = PARITY;
                _serialPort.StopBits = STOPBITS;
                _serialPort.DataReceived += new SerialDataReceivedEventHandler( _serialPort_DataReceivedHandler );

                _serialPort.Open();

                if ( !_serialPort.IsOpen )
                    return false;
            }
            catch
            {
                return false;
            }

            return true;
        }

        private void _initializeSensors()
        {
            if ( _valueMap == null )
                _valueMap = new Dictionary<eSensor, double[]>();

            if ( _sensorMap == null )
                _sensorMap = new Dictionary<eSensor, ISensor>();

            _valueMap.Clear();
            _sensorMap.Clear();

            try
            {
                foreach ( eSensor sensor in eSensor.values() )
                    _valueMap.Add( sensor, new double[] { 0, 0, 0 } );
            }
            catch
            {
                Debug.WriteLine( "Error initializing value map." );
            }

            try
            {
                foreach ( eSensor sensor in eSensor.values() )
                {
                    if ( sensor.type() == eSensorType.Accelerometer )
                        _sensorMap.Add( sensor, new CAccelerometer( sensor.value(), NORMALIZE_ACCEL ) );
                    else if ( sensor.type() == eSensorType.Gyroscope )
                        _sensorMap.Add( sensor, new CGyroscope( sensor.value(), NORMALIZE_GYRO ) );
                }
            }
            catch
            {
                Debug.WriteLine( "Error initializing sensor objects map." );
            }
        }
    }

    public class MockTranslationUnit : ITranslationUnit
    {
        public MockTranslationUnit(){}

        void ITranslationUnit.applyBrake( int brakeID, double brakeValue )
        {
            return;
        }

        bool ITranslationUnit.initialize( string portName )
        {
            return true;
        }

        double[] ITranslationUnit.readSensor( eSensor sensorID, double x, double y, double z )
        {
            return new double[] { x, y, z };
        }
    }
}
