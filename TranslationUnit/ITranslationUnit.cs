namespace TranslationUnit
{
    /// <summary>
    /// TranslationUnit API
    /// </summary>
    public interface ITranslationUnit
    {
        /// <summary>
        /// (Re-)Initializes all parameters and data fields within the Translation Unit
        /// </summary>
        /// <returns> Returns true if the serial port initialized successfully; false otherwise. </returns>
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
        /// Starts the brake thread
        /// </summary>
        /// <returns> Returns true if the brake thread is alive; false otherwise. </returns>
        bool startBrakeThread();

        /// <summary>
        /// Stops the brake thread
        /// </summary>
        /// <returns> Returns true if the brake thread is dead; false otherwise. </returns>
        bool stopBrakeThread();

        /// <summary>
        /// Starts the read thread
        /// </summary>
        /// <returns> Returns true if the read thread is alive; false otherwise. </returns>
        bool startReadThread();

        /// <summary>
        /// Stops the read thread
        /// </summary>
        /// <returns> Returns true if the read thread is dead; false otherwise. </returns>
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
        /// Returns serial port state
        /// </summary>
        /// <returns> Returns true if the serial port is open; false otherwise. </returns>
        bool serialStatus();

        /// <summary>
        /// Returns brake thread state
        /// </summary>
        /// <returns> Returns true if the brake thread is alive; false otherwise. </returns>
        bool brakeThreadStatus();

        /// <summary>
        /// Returns read thread state
        /// </summary>
        /// <returns> Returns true if the read thread is alive; false otherwise. </returns>
        bool readThreadStatus();
    }
}