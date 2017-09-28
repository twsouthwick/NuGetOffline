using NuGet.Common;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
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
        private readonly SourceCacheContext _cache;
        private readonly HttpClientHandler _handler;
        private readonly HttpHandlerResourceV3 _httpResource;
        private readonly SourceRepository _repository;
        private readonly ILogger _logger;
        private readonly FrameworkReducer _reducer = new FrameworkReducer();

        /// <summary>
        /// Initializes a new instance of the <see cref="NuGetOfflineDownloader"/> class.
        /// </summary>
        /// <param name="options">The options to be used while downloading</param>
        public NuGetOfflineDownloader(DownloadOptions options, SourceCacheContext context, ILogger logger)
        {
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
        public async Task RunAsync(DownloadOptions options, IFolder folder, CancellationToken token)
        {
            var desiredFramework = NuGetFramework.ParseFolder(options.Framework, DefaultFrameworkNameProvider.Instance);
            var packages = await GetAllPackagesAsync(options, desiredFramework, token);

            foreach (var package in packages)
            {
                var id = package.NuspecReader.GetId();
                var version = package.NuspecReader.GetVersion();

                Console.WriteLine($"Adding {id}");

                var frameworks = package.GetSupportedFrameworks();
                var needed = _reducer.GetNearest(desiredFramework, frameworks);

                var libs = package.GetLibItems()
                    .Where(item => item.TargetFramework == needed)
                    .SelectMany(item => item.Items);
                var build = package.GetBuildItems()
                    .Where(item => item.TargetFramework == needed)
                    .SelectMany(item => item.Items);

                var items = libs.Concat(build).ToList();

                foreach (var item in items)
                {
                    using (var stream = package.GetStream(item))
                    {
                        var itemPath = Path.Combine(id, version.ToString(), item);

                        await folder.AddAsync(itemPath, stream);
                    }
                }
            }
        }

        private static bool Equals(NuGetFramework framework1, NuGetFramework framework2)
        {
            return DefaultFrameworkNameProvider.Instance.CompareEquivalentFrameworks(framework1, framework2) == 0;
        }

        private async Task<IEnumerable<PackageArchiveReader>> GetAllPackagesAsync(DownloadOptions options, NuGetFramework desiredFramework, CancellationToken token)
        {
            var finder = await _repository.GetResourceAsync<FindPackageByIdResource>();
            var downloadQueue = new Queue<(string name, NuGetVersion version)>();
            downloadQueue.Enqueue((options.Name, NuGetVersion.Parse(options.Version)));
            var seen = new HashSet<string>();

            var result = new List<PackageArchiveReader>();

            while (downloadQueue.Any())
            {
                var entry = downloadQueue.Dequeue();
                var package = await GetPackageAsync(entry.name, entry.version, token);

                result.Add(package);

                var dependencies = package.GetPackageDependencies()
                    .Where(item => Equals(item.TargetFramework, desiredFramework))
                    .SelectMany(item => item.Packages)
                    .ToList();

                foreach (var dependency in dependencies)
                {
                    var versions = await finder.GetAllVersionsAsync(dependency.Id, _cache, _logger, token);
                    var version = dependency.VersionRange.FindBestMatch(versions);

                    downloadQueue.Enqueue((dependency.Id, version));
                }
            }

            return result;
        }

        private async Task<PackageArchiveReader> GetPackageAsync(string name, NuGetVersion version, CancellationToken token)
        {
            var finder = await _repository.GetResourceAsync<FindPackageByIdResource>();
            var ms = new MemoryStream();

            if (await finder.CopyNupkgToStreamAsync(name, version, ms, _cache, _logger, token))
            {
                ms.Position = 0;

                return new PackageArchiveReader(ms);
            }

            throw new NuGetDownloaderException($"Could not find package {name}, v{version}");
        }
    }
}
