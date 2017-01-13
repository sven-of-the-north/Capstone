using System.IO.Ports;

namespace TranslationUnit
{
    public interface ITranslationUnit
    {
        bool initialize( string portName, int baudRate = 9600, int dataBits = 8, Handshake handshake = Handshake.None, Parity parity = Parity.None, StopBits stopBits = StopBits.One );

        void snapshotGlove();

        double[] readSensor( int sensorID );

        void applyBrake( int brakeID, double brakeValue = 1 );
    }
}