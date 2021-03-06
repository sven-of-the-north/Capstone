﻿using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

namespace TranslationUnit
{
#pragma warning disable CS1591
    /// <summary>
    /// Motor bitmasks
    /// </summary>
    public enum eMotor : byte
    {
        none = 0,
        Motor7 = 2,
        Motor6 = 4,
        Motor5 = 8,
        Motor4 = 16,
        Motor3 = 32,
        Motor2 = 64,
        Motor1 = 128,
        RESET = 89
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
#pragma warning restore CS1591

    /// <summary>
    /// Main object used to represent the state of the Force Feedback Glove
    /// </summary>
    public class CTranslationUnit : ITranslationUnit
    {
        private const double NORMALIZE_ACCEL = 0.0001;
        private const double NORMALIZE_GYRO = 0.005493164063;

        private const int CALIBRATION = 30;

        private const int BAUDRATE = 38400;
        private const int DATABITS = 8;
        private const Handshake HANDSHAKE = Handshake.None;
        private const Parity PARITY = Parity.None;
        private const StopBits STOPBITS = StopBits.One;
        private string PORTNAME = "COM4";

        private bool DEBUG_UNITY = true;

        private volatile Dictionary<eSensor, float[]> _valueMap;
        private Dictionary<eSensor, CSensor> _sensorMap;
        private SerialPort _serialPort;

        Thread _brakeThread = null;
        private volatile byte[] _motorCommand = { (byte)eMotor.none };
        private volatile bool _brakeThreadFlag = false;

        Thread _readThread = null;
        private volatile bool _readThreadFlag = false;

        /// <summary>
        /// Constructor for a Translation Unit object
        /// </summary>
        /// <param name="portName"> Serial port name </param>
        public CTranslationUnit( string portName )
        {
            PORTNAME = portName;
        }
        
        /// <summary>
        /// Translation Unit destructor
        /// </summary>
        ~CTranslationUnit()
        {
            _sensorMap.Clear();
            _valueMap.Clear();

            if ( _serialPort != null )
            {
                _serialPort.Close();
                _serialPort.Dispose();
            }

            if ( _brakeThread != null )
                stopBrakeThread();

            if ( _readThread != null )
                stopReadThread();

            if ( DEBUG_UNITY )
                Debug.Log( "TranslationUnit cleaned up!" );
            else
                System.Diagnostics.Debug.WriteLine( "TranslationUnit cleaned up!" );
        }

        /// <summary>
        /// Gets the latest reading from the sensor specified by SensorID
        /// </summary>
        /// <param name="sensorID"> Sensor to read from </param>
        /// <param name="x"> ignored </param>
        /// <param name="y"> ignored </param>
        /// <param name="z"> ignored </param>
        /// <returns> [x, y, z] values read from the specified sensor </returns>
        public float[] readSensor( eSensor sensorID, float x, float y, float z )
        {
            return _valueMap[sensorID];
        }

        /// <summary>
        /// Activates the brake specified by brakeID
        /// </summary>
        /// <param name="brakeID"> Brake to activate </param>
        public void applyBrake( eMotor brakeID )
        {
            _motorCommand[0] |= ( byte )brakeID;
        }

        /// <summary>
        /// Releases the brake specified by brakeID
        /// </summary>
        /// <param name="brakeID"> Brake to deactivate </param>
        public void releaseBrake( eMotor brakeID )
        {
            _motorCommand[0] &= ( byte )( 255 ^ ( byte )brakeID );
        }

        /// <summary>
        /// Returns serial port state
        /// </summary>
        /// <returns> Returns true if the serial port is open; false otherwise. </returns>
        public bool serialStatus()
        {
            if ( _serialPort == null )
                return false;

            return _serialPort.IsOpen;
        }

        /// <summary>
        /// Returns brake thread state
        /// </summary>
        /// <returns> Returns true if the brake thread is alive; false otherwise. </returns>
        public bool brakeThreadStatus()
        {
            if ( _brakeThread == null )
                return false;

            return _brakeThread.IsAlive;
        }

        /// <summary>
        /// Returns read thread state
        /// </summary>
        /// <returns> Returns true if the read thread is alive; false otherwise. </returns>
        public bool readThreadStatus()
        {
            if ( _readThread == null )
                return false;

            return _readThread.IsAlive;
        }

        /// <summary>
        /// Starts the brake thread
        /// </summary>
        /// <returns> Returns true if the brake thread is alive; false otherwise. </returns>
        public bool startBrakeThread()
        {
            if ( _brakeThread != null )
            {
                if ( _brakeThread.IsAlive )
                {
                    if ( stopBrakeThread() ) 
                        _brakeThread = null;
                    else
                        return false;
                }
                else
                {
                    _brakeThread = null;
                }
            }

            _brakeThread = new Thread( new ThreadStart( writeBrakeState ) );
            _brakeThread.IsBackground = true;
            _brakeThreadFlag = true;
            _brakeThread.Start();

            return _brakeThread.IsAlive;
        }

        /// <summary>
        /// Stops the brake thread
        /// </summary>
        /// <returns> Returns true if the brake thread is dead; false otherwise. </returns>
        public bool stopBrakeThread()
        {
            if ( _brakeThread == null )
                return true;

            if ( _brakeThread.IsAlive )
            {
                _brakeThreadFlag = false;
                _brakeThread.Join();
            }

            return !_brakeThread.IsAlive;
        }

        /// <summary>
        /// Starts the read thread
        /// </summary>
        /// <returns> Returns true if the read thread is alive; false otherwise. </returns>
        public bool startReadThread()
        {
            if ( _readThread != null )
            {
                if ( _readThread.IsAlive )
                {
                    if ( stopReadThread() )
                        _readThread = null;
                    else
                        return false;
                }
                else
                {
                    _readThread = null;
                }
            }

            _readThread = new Thread( new ThreadStart( readFromSensors ) );
            _readThread.IsBackground = true;
            _readThreadFlag = true;
            _readThread.Start();

            return _readThread.IsAlive;
        }

        /// <summary>
        /// Stops the read thread
        /// </summary>
        /// <returns> Returns true if the read thread is dead; false otherwise. </returns>
        public bool stopReadThread()
        {
            if ( _readThread == null )
                return true;

            if ( _readThread.IsAlive )
            {
                _readThreadFlag = false;
                _readThread.Join();
            }

            return !_readThread.IsAlive;
        }

        /// <summary>
        /// (Re-)Initializes all parameters and data fields within the Translation Unit
        /// </summary>
        /// <returns> Returns true if the serial port initialized successfully; false otherwise. </returns>
        public bool initialize()
        {
            if ( _serialPort != null )
            {
                _serialPort.Close();
                _serialPort.Dispose();
                _serialPort = null;
            }

            if ( _brakeThread != null )
            {
                if ( !stopBrakeThread() )
                    return false;
            }

            if ( _readThread != null )
            {
                if ( !stopReadThread() )
                    return false;
            }

            _initializeSensors();

            return _initializeSerial();
        }

        /// <summary>
        /// Initializes data fields and sensor objects
        /// </summary>
        private void _initializeSensors()
        {
            if ( _valueMap == null )
                _valueMap = new Dictionary<eSensor, float[]>();

            if ( _sensorMap == null )
                _sensorMap = new Dictionary<eSensor, CSensor>();

            _valueMap.Clear();
            _sensorMap.Clear();

            try
            {
                foreach ( eSensor sensor in eSensor.values() )
                   _valueMap.Add( sensor, new float[] { -1, -1, -1 } );
            }
            catch
            {
                if ( DEBUG_UNITY )
                    Debug.Log( "Error initializing value map." );
                else
                    System.Diagnostics.Debug.WriteLine( "Error initializing value map." );
            }

            try
            {
                
                foreach ( eSensor sensor in eSensor.values() )
                {
                    if ( sensor.type() == eSensorType.Accelerometer )
                        _sensorMap.Add( sensor, new CAccelerometer( ( string )sensor, NORMALIZE_ACCEL ) );
                    else if ( sensor.type() == eSensorType.Gyroscope )
                        _sensorMap.Add( sensor, new CGyroscope( ( string )sensor, NORMALIZE_GYRO ) );
                }
            }
            catch
            {
                if ( DEBUG_UNITY )
                    Debug.Log( "Error initializing sensor objects map." );
                else
                    System.Diagnostics.Debug.WriteLine( "Error initializing sensor objects map." );
            }
        }

        /// <summary>
        /// Initializes serial port
        /// </summary>
        /// <returns> Returns true if the serial port is open; false otherwise. </returns>
        private bool _initializeSerial()
        {
            try
            {
                _serialPort = new SerialPort( PORTNAME );
                _serialPort.BaudRate = BAUDRATE;
                _serialPort.DataBits = DATABITS;
                _serialPort.Handshake = HANDSHAKE;
                _serialPort.Parity = PARITY;
                _serialPort.StopBits = STOPBITS;
                _serialPort.WriteTimeout = 100;
                //_serialPort.DataReceived += new SerialDataReceivedEventHandler( _serialPort_DataReceivedHandler );

                _serialPort.Open();

                if ( !_serialPort.IsOpen )
                    return false;
            }
            catch ( Exception except )
            {
                if ( DEBUG_UNITY )
                {
                    Debug.Log( "Error: \n" + except.Message );
                    Debug.Log( "Stacktrace: \n" + except.StackTrace );
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine( "Error: \n" + except.Message );
                    System.Diagnostics.Debug.WriteLine( "Stacktrace: \n" + except.StackTrace );
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Writes the current brake state to the serial port
        /// </summary>
        private void writeBrakeState()
        {
            while ( _brakeThreadFlag )
            {
                if ( _serialPort.IsOpen )
                {
                    try
                    {
                        _serialPort.Write( _motorCommand, 0, 1 );
                    }
                    catch ( Exception except )
                    {
                        if ( DEBUG_UNITY )
                            Debug.Log( except.StackTrace );
                        else
                            System.Diagnostics.Debug.WriteLine( except.StackTrace );
                    }
                }

                Thread.Sleep( 16 );
            }
        }

        /// <summary>
        /// Polls the serial port, trying to get data
        /// </summary>
        private void readFromSensors()
        {
            const int newLine = 10;
            int readByte = 0;
            int bytesRead = -1;
            byte[] readBytes = new byte[7];
            eSensor sensor = null;

            Dictionary<eSensor, int> calibrations = new Dictionary<eSensor, int>();
            foreach ( eSensor elem in eSensor.values() )
                calibrations.Add( elem, 0 );

            while ( _readThreadFlag )
            {
                for ( int i = 0; i < 8; ++i )
                {
                    try
                    {
                        readByte = _serialPort.ReadByte();
                        if ( ( readByte ^ newLine ) != 0 )
                            break;
                    }
                    catch ( Exception )
                    {
                        break;
                    }

                    try
                    {
                        bytesRead = _serialPort.Read( readBytes, 0, 7 );
                        sensor = ( eSensor )readBytes[0];
                    }
                    catch ( InvalidCastException )
                    {
                        //This is going to happen a lot
                        continue;
                    }
                    catch ( Exception except )
                    {
                        if ( DEBUG_UNITY )
                            Debug.Log( except.StackTrace );
                        else
                            System.Diagnostics.Debug.WriteLine( except.StackTrace );

                        continue;
                    }

                    if ( ( bytesRead == 7 ) && _sensorMap.ContainsKey( sensor ) )
                    {
                        try
                        {
                            int raw_x = BitConverter.ToInt16( readBytes, 1 );
                            int raw_y = BitConverter.ToInt16( readBytes, 3 );
                            int raw_z = BitConverter.ToInt16( readBytes, 5 );

                            double[] processed = _sensorMap[sensor].getValue( new int[] { raw_x, raw_y, raw_z } );

                            if ( ( calibrations[sensor] <= CALIBRATION ) && ( sensor.type() == eSensorType.Gyroscope ) )
                            {
                                if ( calibrations[sensor] == CALIBRATION )
                                    _sensorMap[sensor].setOffsets( processed[0], processed[1], processed[2] );
                                calibrations[sensor]++;
                            }

                            _valueMap[sensor] = new float[] { ( float )-processed[1], ( float )-processed[2], ( float )-processed[0] };

                        }
                        catch ( FormatException )
                        {
                            //This is going to happen a lot
                            continue;
                        }
                        catch ( Exception except )
                        {
                            if ( DEBUG_UNITY )
                                Debug.Log( except.StackTrace );
                            else
                                System.Diagnostics.Debug.WriteLine( except.StackTrace );

                            continue;
                        }
                    }
                }

                Thread.Sleep( 5 );
            }
        }

        /*
        /// <summary>
        /// Handler for receiving serial data objects. THIS DOESNT WORK IN UNITY
        /// </summary>
        /// <param name="sender"> ignored </param>
        /// <param name="e"> ignored </param>
        private void _serialPort_DataReceivedHandler( object sender, SerialDataReceivedEventArgs e )
        {
            Debug.Log( "Handling data received event" );

            string rawData = "";
            string[] data = { };
            eSensor sensor = null;

            try
            {
                rawData = _serialPort.ReadLine();
                data = rawData.Split( ',' );
                sensor = ( eSensor )data[0];

                Debug.Log( "Read from sensor" );
            }
            catch ( InvalidCastException )
            {
                //This is going to happen a lot
                return;
            }
            catch ( Exception except )
            {
                if ( DEBUG_UNITY )
                    Debug.Log( except.StackTrace );
                else
                    System.Diagnostics.Debug.WriteLine( except.StackTrace );

                return;
            }

            if ( ( data.Length == 4 ) && _sensorMap.ContainsKey( sensor ) )
            {
                try
                {
                    Debug.Log( "Read good data" );

                    data[3] = data[3].Remove( data[3].Length - 1, 1 ); // strip newline character

                    double raw_x = Convert.ToDouble(data[1]);
                    double raw_y = Convert.ToDouble(data[2]);
                    double raw_z = Convert.ToDouble(data[3]);

                    double[] processed = _sensorMap[sensor].getValue( new double[] { raw_x, raw_y, raw_z } );

                    _valueMap[sensor] = new float[] { ( float )processed[0], ( float )processed[1], ( float )processed[2] };
                    Debug.Log( "Wrote good data" );
                }
                catch ( FormatException )
                {
                    //This is going to happen a lot
                    return;
                }
                catch ( Exception except )
                {
                    if ( DEBUG_UNITY )
                        Debug.Log( except.StackTrace );
                    else
                        System.Diagnostics.Debug.WriteLine( except.StackTrace );

                    return;
                }
            }
        }
        */
    }

    /// <summary>
    /// Mock object for testing with TranslationUnit, even without a serial port. ** MAY NOT BE FULLY IMPLEMENTED **
    /// </summary>
    public class MockTranslationUnit : ITranslationUnit
    {
        bool _initialized = false;
        bool _readThreadState = false;
        bool _writeThreadState = false;

        /// <summary>
        /// MockTranslationUnit constructor - empty object created
        /// </summary>
        public MockTranslationUnit(){}

        /// <summary>
        /// Pretends to apply a brake - does nothing
        /// </summary>
        /// <param name="brakeID"> Ignored </param>
        public void applyBrake( eMotor brakeID )
        {
            return;
        }

        /// <summary>
        /// Pretends to release a brake - does nothing
        /// </summary>
        /// <param name="brakeID"> Ignored </param>
        public void releaseBrake( eMotor brakeID )
        {
            return;
        }

        /// <summary>
        /// Pretends to start the brake thread - sets writeThreadState flag to true if it was false
        /// Requires the initialized flag to be true
        /// </summary>
        /// <returns> Returns true if the internal writeThreadState was false </returns>
        public bool startBrakeThread()
        {
            if ( !_initialized )
                return false;

            if ( !_writeThreadState )
            {
                _writeThreadState = true;
                return _writeThreadState;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Pretends to stop the brake thread - sets writeThreadState flag to false if it was true
        /// Requires the initialized flag to be true
        /// </summary>
        /// <returns> Returns true if the internal writeThreadState was true; false otherwise </returns>
        public bool stopBrakeThread()
        {
            if ( !_initialized )
                return false;

            if ( _writeThreadState )
            {
                _writeThreadState = false;
                return !_writeThreadState;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Pretends to start the read thread - sets readThreadState flag to true if it was false
        /// Requires the initialized flag to be true
        /// </summary>
        /// <returns> Returns true if the internal readThreadState was false </returns>
        public bool startReadThread()
        {
            if ( !_initialized )
                return false;

            if ( !_readThreadState )
            {
                _readThreadState = true;
                return _readThreadState;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Pretends to stop the read thread - sets readThreadState flag to false if it was true
        /// Requires the initialized flag to be true
        /// </summary>
        /// <returns> Returns true if the internal readThreadState was true </returns>
        public bool stopReadThread()
        {
            if ( !_initialized )
                return false;

            if ( _readThreadState )
            {
                _readThreadState = false;
                return !_readThreadState;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Pretends to initialize the TranslationUnit - sets initialized flag to true
        /// </summary>
        /// <returns> Returns initialized flag status </returns>
        public bool initialize()
        {
            _initialized = true;
            return _initialized;
        }

        /// <summary>
        /// Pretends to check the serial port status
        /// Requires the initialized flag to be true
        /// </summary>
        /// <returns> Returns true </returns>
        public bool serialStatus()
        {
            if ( !_initialized )
                return false;

            return true;
        }

        /// <summary>
        /// Pretends to check the brake thread status
        /// Requires the initialized flag to be true
        /// </summary>
        /// <returns></returns>
        public bool brakeThreadStatus()
        {
            if ( !_initialized )
                return false;

            return _writeThreadState;
        }

        /// <summary>
        /// Pretends to check the read thread status
        /// Requires the initialized flag to be true
        /// </summary>
        /// <returns></returns>
        public bool readThreadStatus()
        {
            if ( !_initialized )
                return false;

            return _readThreadState;
        }

        /// <summary>
        /// Pretends to read a sensor - echoes the passed arguments 
        /// Requires the initialized flag to be true
        /// </summary>
        /// <param name="sensorID"> Ignored </param>
        /// <param name="x"> X value to echo </param>
        /// <param name="y"> Y value to echo </param>
        /// <param name="z"> Z value to echo </param>
        /// <returns> The x, y, z values that were passed </returns>
        public float[] readSensor( eSensor sensorID, float x, float y, float z )
        {
            if ( !_initialized )
                return new float[] { float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity };

            return new float[] { x, y, z };
        }
    }
}
