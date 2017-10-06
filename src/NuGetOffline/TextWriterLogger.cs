using System;
using System.IO;

namespace NuGetOffline
{
    /// <summary>
    /// A NuGet logger that writes to a TextWriter
    /// </summary>
    internal class TextWriterLogger : ILogger, NuGet.Common.ILogger
    {
        private readonly TextWriter _writer;
        private readonly DownloadOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextWriterLogger"/> class.
        /// </summary>
        /// <param name="writer">TextWriter to write output to</param>
        public TextWriterLogger(TextWriter writer, DownloadOptions options)
        {
            _writer = writer;
            _options = options;
        }

        private enum Level
        {
            Warning,
            Info,
            Debug,
            Verbose,
            Minimal,
            Summary,
            ErrorSummary,
            Error
        }

        /// <inheritdoc/>
        public void Info(string message) => Write(Level.Info, message);

        private void Write(Level level, string message)
        {
            void Write()
            {
                _writer.WriteLine($"[{level}] {message}");
            }

            if (_options.Verbose)
            {
                Write();
            }
            else if (!RequiresVerbose(level))
            {
                Write();
            }
        }

        /// <inheritdoc/>
        public void LogDebug(string data) => Write(Level.Debug, data);

        /// <inheritdoc/>
        public void LogError(string data) => Write(Level.Error, data);

        /// <inheritdoc/>
        public void LogErrorSummary(string data) => Write(Level.ErrorSummary, data);

        /// <inheritdoc/>
        public void LogInformation(string data) => Write(Level.Verbose, data);

        /// <inheritdoc/>
        public void LogInformationSummary(string data) => Write(Level.Summary, data);

        /// <inheritdoc/>
        public void LogMinimal(string data) => Write(Level.Minimal, data);

        /// <inheritdoc/>
        public void LogVerbose(string data) => Write(Level.Verbose, data);

        /// <inheritdoc/>
        public void LogWarning(string data) => Write(Level.Warning, data);

        /// <inheritdoc/>
        public void Verbose(string message) => Write(Level.Verbose, message);

        private static bool RequiresVerbose(Level level)
        {
            switch (level)
            {
                case Level.Minimal:
                case Level.Verbose:
                case Level.Debug:
                    return true;
                case Level.Warning:
                case Level.Info:
                case Level.Summary:
                case Level.Error:
                case Level.ErrorSummary:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, "Unknown verbosity level");
            }
        }
    }
}
