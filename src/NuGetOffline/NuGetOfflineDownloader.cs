using System.Threading.Tasks;

namespace NuGetOffline
{
    /// <summary>
    /// Contains entry point for main application logic called from <see cref="Program.Main(string[])"/>
    /// </summary>
    public class NuGetOfflineDownloader
    {
        /// <summary>
        /// Entry point for main application logic called from <see cref="Program.Main(string[])"/>
        /// </summary>
        public Task RunAsync()
        {
            return Task.CompletedTask;
        }
    }
}
