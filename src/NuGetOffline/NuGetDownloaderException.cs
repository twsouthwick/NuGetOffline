using System;

namespace NuGetOffline
{
    /// <summary>
    /// An exception for NuGet downloader exceptions. Exceptions of this type or derived classes will not output
    /// stacktrace information; they will be considered expected
    /// </summary>
    public class NuGetDownloaderException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NuGetDownloaderException"/> class.
        /// </summary>
        /// <param name="message">Exception message</param>
        public NuGetDownloaderException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NuGetDownloaderException"/> class.
        /// </summary>
        public NuGetDownloaderException()
        {
        }
    }
}
