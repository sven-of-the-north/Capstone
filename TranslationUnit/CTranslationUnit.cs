using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace TranslationUnit
{
    enum eSensor
    {
        Hand = 0,
        Thumb_Distal,
        Thumb_Proximal,
        Thumb_Pressure,
        Index_Middle,
        Index_Proximal,
        Index_Pressure,
        Middle_Middle,
        Middle_Proximal,
        Middle_Pressure
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

        CTranslationUnit( string portName, int baudRate = 9600, int dataBits = 8, Handshake handshake = Handshake.None, Parity parity = Parity.None, StopBits stopBits = StopBits.One )
        {
            if ( !_initialize( portName, baudRate, dataBits, handshake, parity, stopBits ) )
                throw new InvalidSerialPortException( "Could not open serial port for communication. Are all the parameters given correct?" );
        }

        ~CTranslationUnit()
        {
            _sensorMap.Clear();
            _serialPort.Dispose();
        }

        void ITranslationUnit.snapshotGlove()
        {
            _inputMap.Clear();

            //TODO: Actual read method. This is a temporary method that just pretends to read stuff.
            //Needs try/catch
            //string rawData = _serialPort.ReadExisting();
            //double[] dblData = Array.ConvertAll(rawData.split(','), Double.Parse);

            foreach ( eSensor id in Enum.GetValues( typeof( eSensor ) ) )
                _inputMap.Add( id, new double[] { 0.0, 0.0, 0.0 } );

            foreach ( eSensor id in Enum.GetValues( typeof( eSensor ) ) ) 
                _valueMap[id] = _sensorMap[id].getValue( _inputMap[id] );
        }

        double[] ITranslationUnit.readSensor( int sensorID )
        {
            return _valueMap[( eSensor )sensorID];
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
            throw new NotImplementedException();
        }

        private bool _initialize( string portName, int baudRate, int dataBits, Handshake handshake, Parity parity, StopBits stopBits )
        {
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

            _initializeSensors();
            _gravityCompensator = new CGravityCompensator();

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

            foreach ( eSensor id in Enum.GetValues( typeof( eSensor ) ) )
                _inputMap.Add( id, new double[] { 0.5, 0.5, 0.5 } );

            foreach ( eSensor id in Enum.GetValues( typeof( eSensor ) ) )
                _inputMap.Add( id, new double[] { 0.0, 0.0, 0.0 } );

            _sensorMap.Add( eSensor.Hand, new CKinematicSensor( null ) );
            _sensorMap.Add( eSensor.Thumb_Distal, new CKinematicSensor( null ) );
            _sensorMap.Add( eSensor.Thumb_Proximal, new CKinematicSensor( _sensorMap[eSensor.Thumb_Distal] ) );
            _sensorMap.Add( eSensor.Thumb_Pressure, new CPressureSensor() );
            _sensorMap.Add( eSensor.Index_Middle, new CKinematicSensor( null ) );
            _sensorMap.Add( eSensor.Index_Proximal, new CKinematicSensor( _sensorMap[eSensor.Index_Middle] ) );
            _sensorMap.Add( eSensor.Index_Pressure, new CPressureSensor() );
            _sensorMap.Add( eSensor.Middle_Middle, new CKinematicSensor( null ) );
            _sensorMap.Add( eSensor.Middle_Proximal, new CKinematicSensor( _sensorMap[eSensor.Middle_Middle] ) );
            _sensorMap.Add( eSensor.Middle_Pressure, new CPressureSensor() );
        }
    }
}
