namespace TranslationUnit
{
    internal interface ISensor
    {
        double[] getValue( double[] input );
        double[] getState();
    }
}