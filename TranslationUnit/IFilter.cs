namespace TranslationUnit
{
    interface IFilter
    {
        /// <summary>
        /// Updates internal state and computes filtered output
        /// </summary>
        /// <param name="input">Value to be filtered</param>
        /// <returns>Filtered output value, based on previous state</returns>
        double[] filter( double input );

        /// <summary>
        /// Does no computation, simply returns the last calculated output value
        /// </summary>
        /// <returns> Last calculated output value </returns>
        double[] getState();
    }
}
