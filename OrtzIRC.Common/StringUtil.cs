namespace OrtzIRC.Common
{
    using System.Text;

    public static class StringUtil
    {
        /// <summary>
        /// Makes string.Format a little cleaner.
        /// </summary>
        /// <param name="format">The string to be formatted.</param>
        /// <param name="args">The objects to format with.</param>
        /// <returns>The final formatted string.</returns>
        /// <example>
        /// string phoneFormat = "({0}) {1}-{2}"; <br />
        /// return phoneFormat.With(areaCode, prefix, number);
        /// </example>
        public static string With(this string format, params object[] args)
        {
            return string.Format(format, args);
        }

        public static byte[] GetBytes(this string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }

        public static string FromBytes(this string str, byte[] array)
        {
            return Encoding.ASCII.GetString(array);
        }
    }
}