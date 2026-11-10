using System;

namespace Kuoste.LasZipNetStandard
{
    /// <summary>
    /// Thrown when a native LASzip call fails. The message includes LASzip's own error text when available.
    /// </summary>
    public class LasZipException : Exception
    {
        public LasZipException(string message) : base(message)
        {
        }
    }
}
