namespace TranslationUnit
{
    internal interface ISensor
    {
        /// <summary>
        /// All input values are run through their own set of stateful filters that can be defined in the constructor for this class
        /// </summary>
        /// <param name="input"> Raw [x, y, z] values </param>
        /// <returns> Calculated [x, y, z] values </returns>
        double[] getValue( int[] input );

        /// <summary>
        /// Does no computation, simply returns the last calculated output values
        /// </summary>
        /// <returns> Last calculated output values [x, y, z] </returns>
        double[] getState();
    }
}