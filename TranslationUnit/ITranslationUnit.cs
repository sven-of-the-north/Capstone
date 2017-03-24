namespace TranslationUnit
{
    public interface ITranslationUnit
    {
        /// <summary>
        /// (Re-)Initializes all parameters and data fields within the Translation Unit
        /// </summary>
        /// <param name="portName"> Serial port name </param>
        /// <returns> Initialization successful or not </returns>
        bool initialize();

        /// <summary>
        /// Gets the latest reading from the sensor specified by SensorID
        /// </summary>
        /// <param name="sensorID"> Sensor to read from </param>
        /// <param name="x"> X-value to echo (for debug) </param>
        /// <param name="y"> Y-value to echo (for debug) </param>
        /// <param name="z"> Z-value to echo (for debug) </param>
        /// <returns> [x, y, z] values read from the specified sensor </returns>
        float[] readSensor( eSensor sensorID, float x = -1, float y = -1, float z = -1 );

        /// <summary>
        /// Starts writing to the brakes
        /// </summary>
        /// <returns>True if the brake thread is started; False if the brake thread is still dead</returns>
        bool startBrakeThread();

        /// <summary>
        /// Stops writing to the brakes
        /// </summary>
        /// <returns>True if the brake thread is stopped; False if the brake thread is still alive</returns>
        bool stopBrakeThread();

        /// <summary>
        /// Starts polling the serial port
        /// </summary>
        /// <returns></returns>
        bool startReadThread();

        /// <summary>
        /// Stops polling the serial port
        /// </summary>
        /// <returns></returns>
        bool stopReadThread();

        /// <summary>
        /// Activates the brake specified by brakeID
        /// </summary>
        /// <param name="brakeID"> Brake to activate </param>
        void applyBrake( eMotor brakeID );

        /// <summary>
        /// Releases the brake specified by brakeID
        /// </summary>
        /// <param name="brakeID"> Brake to deactivate </param>
        void releaseBrake( eMotor brakeID );

        /// <summary>
        /// Returns serial port status
        /// </summary>
        /// <returns></returns>
        bool serialStatus();

        /// <summary>
        /// Returns if the brake thread is alive
        /// </summary>
        /// <returns></returns>
        bool brakeThreadStatus();

        /// <summary>
        /// Returns if the read thread is alive
        /// </summary>
        /// <returns></returns>
        bool readThreadStatus();
    }
}