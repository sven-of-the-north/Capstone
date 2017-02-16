using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

namespace TranslationUnit
{
    public enum eSensor
    {
        Hand = 0,
        Thumb_Proximal,
        Thumb_Distal,
        Index_Proximal,
        Index_Middle,
        Middle_Proximal,
        Middle_Middle,
    };

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

    public class CTranslationUnit : ITranslationUnit
    {
        private Dictionary<eSensor, double[]> _inputMap;
        private Dictionary<eSensor, double[]> _valueMap;
        private Dictionary<eSensor, ISensor> _sensorMap;
        private SerialPort _serialPort;
        private CGravityCompensator _gravityCompensator;

        private AutoResetEvent autoEvent = new AutoResetEvent(false);
        private Timer _readTimer;

        public CTranslationUnit( string portName, int baudRate = 9600, int dataBits = 8, Handshake handshake = Handshake.None, Parity parity = Parity.None, StopBits stopBits = StopBits.One )
        {
            if ( !_initialize( portName, baudRate, dataBits, handshake, parity, stopBits ) )
                throw new InvalidSerialPortException( "Could not open serial port for communication. Are all the parameters given correct?" );
        }

        ~CTranslationUnit()
        {
            _sensorMap.Clear();
            _serialPort.Dispose();
            _readTimer.Dispose();
        }

        double[] ITranslationUnit.readSensor( int sensorID, bool debug, double x, double y, double z )
        {
            if ( !debug )
                return _valueMap[( eSensor )sensorID];

            return new double[] { x, y, z };
        }

        void ITranslationUnit.applyBrake( int brakeID, double brakeValue )
        {
            throw new NotImplementedException();
        }

        bool ITranslationUnit.initialize( string portName, int baudRate, int dataBits, Handshake handshake, Parity parity, StopBits stopBits )
        {
            return _initialize( portName, baudRate, dataBits, handshake, parity, stopBits );
        }

        void _serialPort_DataReceivedHandler( object sender, SerialDataReceivedEventArgs e )
        {
            string rawData;
            try
            {
                rawData = _serialPort.ReadLine();
            }
            catch
            {
                return;
            }

            Debug.Print( rawData );
            double[] dblData = Array.ConvertAll(rawData.Split(';'), Double.Parse);

            _inputMap[eSensor.Hand] = new double[] { dblData[0], dblData[1], dblData[2] };
            _inputMap[eSensor.Thumb_Proximal] = new double[] { dblData[3], dblData[4], dblData[5] };
            _inputMap[eSensor.Thumb_Distal] = new double[] { dblData[6], dblData[7], dblData[8] };
            _inputMap[eSensor.Index_Proximal] = new double[] { dblData[9], dblData[10], dblData[11] };
            _inputMap[eSensor.Index_Middle] = new double[] { dblData[12], dblData[13], dblData[14] };
            _inputMap[eSensor.Middle_Proximal] = new double[] { dblData[15], dblData[16], dblData[17] };
            _inputMap[eSensor.Middle_Middle] = new double[] { dblData[18], dblData[19], dblData[20] };

            // Update calculations for each sensor
            foreach ( eSensor id in Enum.GetValues( typeof( eSensor ) ) )
                _valueMap[id] = _sensorMap[id].getValue( _inputMap[id] );
        }

        void _snapshotGlove( Object stateInfo )
        {
            string rawData;
            try
            {
                rawData = _serialPort.ReadLine();

            }
            catch
            {
                return;
            }

            Debug.Print( rawData );
            double[] dblData = Array.ConvertAll(rawData.Split(';'), Double.Parse);

            _inputMap[eSensor.Hand] = new double[] { dblData[0], dblData[1], dblData[2] };
            _inputMap[eSensor.Thumb_Proximal] = new double[] { dblData[3], dblData[4], dblData[5] };
            _inputMap[eSensor.Thumb_Distal] = new double[] { dblData[6], dblData[7], dblData[8] };
            _inputMap[eSensor.Index_Proximal] = new double[] { dblData[9], dblData[10], dblData[11] };
            _inputMap[eSensor.Index_Middle] = new double[] { dblData[12], dblData[13], dblData[14] };
            _inputMap[eSensor.Middle_Proximal] = new double[] { dblData[15], dblData[16], dblData[17] };
            _inputMap[eSensor.Middle_Middle] = new double[] { dblData[18], dblData[19], dblData[20] };

            // Update calculations for each sensor
            foreach ( eSensor id in Enum.GetValues( typeof( eSensor ) ) )
                _valueMap[id] = _sensorMap[id].getValue( _inputMap[id] );
        }

        private bool _initialize( string portName, int baudRate, int dataBits, Handshake handshake, Parity parity, StopBits stopBits )
        {
            _initializeSensors();
            _gravityCompensator = new CGravityCompensator();

            try
            {
                _serialPort = new SerialPort( portName );
                _serialPort.BaudRate = baudRate;
                _serialPort.DataBits = dataBits;
                _serialPort.Handshake = handshake;
                _serialPort.Parity = parity;
                _serialPort.StopBits = stopBits;
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
            if ( _inputMap == null )
                _inputMap = new Dictionary<eSensor, double[]>();

            if ( _valueMap == null )
                _valueMap = new Dictionary<eSensor, double[]>();

            if ( _sensorMap == null )
                _sensorMap = new Dictionary<eSensor, ISensor>();

            _inputMap.Clear();
            _valueMap.Clear();
            _sensorMap.Clear();
            
            try
            {
                foreach ( eSensor id in Enum.GetValues( typeof( eSensor ) ) )
                    _inputMap.Add( id, new double[] { 0.0, 0.0, 0.0 } );
            }
            catch
            {
                Debug.WriteLine( "Error initializing input values map." );
            }

            try
            {
                _sensorMap.Add( eSensor.Hand, new CKinematicSensor( null ) );
                _sensorMap.Add( eSensor.Thumb_Proximal, new CKinematicSensor( _sensorMap[eSensor.Hand] ) );
                _sensorMap.Add( eSensor.Thumb_Distal, new CKinematicSensor( _sensorMap[eSensor.Thumb_Proximal] ) );
                _sensorMap.Add( eSensor.Index_Proximal, new CKinematicSensor( _sensorMap[eSensor.Hand] ) );
                _sensorMap.Add( eSensor.Index_Middle, new CKinematicSensor( _sensorMap[eSensor.Index_Middle] ) );
                _sensorMap.Add( eSensor.Middle_Proximal, new CKinematicSensor( _sensorMap[eSensor.Hand] ) );
                _sensorMap.Add( eSensor.Middle_Middle, new CKinematicSensor( _sensorMap[eSensor.Middle_Middle] ) );
            }
            catch
            {
                Debug.WriteLine( "Error initializing sensor objects map." );
            }

            if ( _readTimer == null )
            {
                try
                {
                    _readTimer = new Timer( this._snapshotGlove, autoEvent, 500, 15 );
                }
                catch
                {
                    Debug.WriteLine( "Error initializing timer thread." );
                }
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

        bool ITranslationUnit.initialize( string portName, int baudRate, int dataBits, Handshake handshake, Parity parity, StopBits stopBits )
        {
            return true;
        }

        double[] ITranslationUnit.readSensor( int sensorID, bool debug, double x, double y, double z )
        {
            return new double[] { x, y, z };
        }
    }
}
