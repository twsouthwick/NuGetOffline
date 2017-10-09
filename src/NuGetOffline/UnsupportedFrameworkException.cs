using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using System.Collections.Generic;
using System.Linq;

namespace NuGetOffline
{
    /// <summary>
    /// Exception for frameworks that are not supported
    /// </summary>
    public class UnsupportedFrameworkException : NuGetDownloaderException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnsupportedFrameworkException"/> class.
        /// </summary>
        /// <param name="package">Package that could not be used</param>
        /// <param name="framework">Desired framework</param>
        public UnsupportedFrameworkException(PackageArchiveReader package, NuGetFramework framework)
        {
            PackageId = package.GetIdentity();
            DesiredFramework = framework;
            SupportedFrameworks = package.GetSupportedFrameworks().ToList();
        }

        /// <summary>
        /// Gets the package identity
        /// </summary>
        public PackageIdentity PackageId { get; }

        /// <summary>
        /// Gets the desired framework
        /// </summary>
        public NuGetFramework DesiredFramework { get; }

        /// <summary>
        /// Gets the supported frameworks for the package
        /// </summary>
        public IReadOnlyCollection<NuGetFramework> SupportedFrameworks { get; }

        /// <inheritdoc/>
        public override string Message => $"Required package '{PackageId.Id}' does not support framework {DesiredFramework}. Supported frameworks: {string.Join(",", SupportedFrameworks)}";
    }
}
