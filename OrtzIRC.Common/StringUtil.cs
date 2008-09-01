namespace OrtzIRC.Common
{
    public static class StringUtil
    {
        /// <summary>
        /// Makes string.Format a little cleaner.
        /// </summary>
        /// <param name="format">The string to be formatted.</param>
        /// <param name="args">The strings to format with.</param>
        /// <returns>The final formatted string.</returns>
        /// <example>
        /// string phoneFormat = "({0}) {1}-{2}"; <br />
        /// return phoneFormat.With(areaCode, prefix, number);
        /// </example>
        public static string With(this string format, params string[] args)
        {
            return string.Format(format, args);
        }
    }
}