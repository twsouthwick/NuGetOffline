namespace NuGetOffline
{
    /// <summary>
    /// An abstraction for logging
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Write an information level log
        /// </summary>
        /// <param name="message">Message to write out</param>
        void Info(string message);

        /// <summary>
        /// Write a verbose level log
        /// </summary>
        /// <param name="message">Message to write out</param>
        void Verbose(string message);
    }
}
