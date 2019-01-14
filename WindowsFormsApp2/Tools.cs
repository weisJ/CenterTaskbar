namespace CenterTaskbar
{
    using System;

    /// <summary>
    /// Defines the <see cref="Tools" />
    /// </summary>
    internal static class Tools
    {
        /// <summary>
        /// Checks whether two double values are approximately equal to accommodate floating point errors.
        /// </summary>
        /// <param name="a">first double</param>
        /// <param name="b">second double</param>
        /// <returns>true of the difference is in a margin of error.</returns>
        public static bool AreSimilar(double a, double b)
        {
            return Math.Abs(a - b) < 0.00001;
        }
    }
}