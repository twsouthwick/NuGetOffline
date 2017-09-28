using NuGet.Common;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NuGetOffline
{
    /// <summary>
    /// Contains entry point for main application logic called from <see cref="Program.Main(string[])"/>
    /// </summary>
    public sealed class NuGetOfflineDownloader : IDisposable
    {
        private readonly DownloadOptions _options;
        private readonly SourceCacheContext _cache;
        private readonly HttpClientHandler _handler;
        private readonly HttpHandlerResourceV3 _httpResource;
        private readonly SourceRepository _repository;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NuGetOfflineDownloader"/> class.
        /// </summary>
        /// <param name="options">The options to be used while downloading</param>
        public NuGetOfflineDownloader(DownloadOptions options, SourceCacheContext context, ILogger logger)
        {
            _options = options;
            _cache = context;
            _handler = new HttpClientHandler();
            _httpResource = new HttpHandlerResourceV3(_handler, _handler);
            _repository = Repository.Factory.GetCoreV3(options.Feed);
            _logger = logger;
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            _handler.Dispose();
        }

        /// <summary>
        /// Entry point for main application logic called from <see cref="Program.Main(string[])"/>
        /// </summary>
        public async Task RunAsync(IFolder folder, CancellationToken token)
        {
            var package = await GetPackageAsync(token);

            var frameworks = package.GetSupportedFrameworks();
            var reducer = new FrameworkReducer();
            var desired = NuGetFramework.ParseFolder(_options.Framework, DefaultFrameworkNameProvider.Instance);
            var needed = reducer.GetNearest(desired, frameworks);

            var libs = package.GetLibItems()
                .Where(item => item.TargetFramework == needed)
                .SelectMany(item => item.Items);
            var build = package.GetBuildItems()
                .Where(item => item.TargetFramework == needed)
                .SelectMany(item => item.Items);

            var items = libs.Concat(build);

            foreach (var item in items)
            {
                using (var stream = package.GetStream(item))
                {
                    var name = Path.GetFileName(item);
                    var itemPath = Path.Combine(_options.Name, _options.Version, name);

                    await folder.AddAsync(itemPath, stream);
                }
            }
        }

        private async Task<PackageArchiveReader> GetPackageAsync(CancellationToken token)
        {
            var finder = await _repository.GetResourceAsync<FindPackageByIdResource>();
            var nugetVersion = NuGetVersion.Parse(_options.Version);
            var ms = new MemoryStream();

            if (await finder.CopyNupkgToStreamAsync(_options.Name, nugetVersion, ms, _cache, _logger, token))
            {
                ms.Position = 0;

                return new PackageArchiveReader(ms);
            }

            throw new NuGetDownloaderException($"Could not find package {_options.Name}, v{_options.Version}");
        }
    }
}
