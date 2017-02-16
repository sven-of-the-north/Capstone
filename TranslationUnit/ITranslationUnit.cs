using System.IO.Ports;

namespace TranslationUnit
{
    public interface ITranslationUnit
    {
        bool initialize( string portName, int baudRate = 9600, int dataBits = 8, Handshake handshake = Handshake.None, Parity parity = Parity.None, StopBits stopBits = StopBits.One );

        double[] readSensor( int sensorID, bool debug = false, double x = -1, double y = -1, double z = -1 );

        void applyBrake( int brakeID, double brakeValue = 1 );
    }
}