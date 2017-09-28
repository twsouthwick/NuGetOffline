using NuGet.Common;
using System.IO;

namespace NuGetOffline
{
    /// <summary>
    /// A NuGet logger that writes to a TextWriter
    /// </summary>
    internal class NuGetTextWriterLogger : ILogger
    {
        private readonly TextWriter _writer;

        /// <summary>
        /// Initializes a new instance of the <see cref="NuGetTextWriterLogger"/> class.
        /// </summary>
        /// <param name="writer">TextWriter to write output to</param>
        public NuGetTextWriterLogger(TextWriter writer)
        {
            _writer = writer;
        }

        /// <inheritdoc/>
        public void LogDebug(string data) => _writer.WriteLine($"[Debug] {data}");

        /// <inheritdoc/>
        public void LogError(string data) => _writer.WriteLine($"[Error] {data}");

        /// <inheritdoc/>
        public void LogErrorSummary(string data) => _writer.WriteLine($"[Error Summary] {data}");

        /// <inheritdoc/>
        public void LogInformation(string data) => _writer.WriteLine($"[Info] {data}");

        /// <inheritdoc/>
        public void LogInformationSummary(string data) => _writer.WriteLine($"[Info Summary] {data}");

        /// <inheritdoc/>
        public void LogMinimal(string data) => _writer.WriteLine($"[Minimal] {data}");

        /// <inheritdoc/>
        public void LogVerbose(string data) => _writer.WriteLine($"[Verbose] {data}");

        /// <inheritdoc/>
        public void LogWarning(string data) => _writer.WriteLine($"[Warning] {data}");
    }
}
