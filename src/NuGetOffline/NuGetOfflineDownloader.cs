using System.Threading.Tasks;

namespace NuGetOffline
{
    /// <summary>
    /// Contains entry point for main application logic called from <see cref="Program.Main(string[])"/>
    /// </summary>
    public class NuGetOfflineDownloader
    {
        private readonly DownloadOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="NuGetOfflineDownloader"/> class.
        /// </summary>
        /// <param name="options">The options to be used while downloading</param>
        public NuGetOfflineDownloader(DownloadOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// Entry point for main application logic called from <see cref="Program.Main(string[])"/>
        /// </summary>
        public Task RunAsync()
        {
            return Task.CompletedTask;
        }
    }
}
