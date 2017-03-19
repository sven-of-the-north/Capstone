using System.IO.Ports;

namespace TranslationUnit
{
    public interface ITranslationUnit
    {
        /// <summary>
        /// (Re-)Initializes all parameters and data fields within the Translation Unit
        /// </summary>
        /// <param name="portName"> Serial port name </param>
        /// <returns> Initialization successful or not </returns>
        bool initialize( string portName );

        /// <summary>
        /// Gets the latest reading from the sensor specified by SensorID
        /// </summary>
        /// <param name="sensorID"> Sensor to read from </param>
        /// <param name="x"> X-value to echo (for debug) </param>
        /// <param name="y"> Y-value to echo (for debug) </param>
        /// <param name="z"> Z-value to echo (for debug) </param>
        /// <returns> [x, y, z] values read from the specified sensor </returns>
        double[] readSensor( eSensor sensorID, double x = -1, double y = -1, double z = -1 );

        /// <summary>
        /// Activates the brake specified by brakeID
        /// </summary>
        /// <param name="brakeID"> Brake to activate </param>
        /// <param name="brakeValue"> Magnitude of braking force (default 1 - 100%) </param>
        void applyBrake( int brakeID, double brakeValue = 1 );
    }
}