using NuGet.Frameworks;
using NuGet.Packaging;
using System.Collections.Generic;
using System.Linq;

namespace NuGetOffline
{
    /// <summary>
    /// Collection of extension methods to simplify NuGet packages
    /// </summary>
    internal static class NuGetExtensions
    {
        /// <summary>
        /// Get items from a package for a given framework
        /// </summary>
        /// <param name="package">Package to search in</param>
        /// <param name="framework">Framework to search for</param>
        /// <returns>A collection of paths with the package</returns>
        public static IEnumerable<string> GetFrameworkItems(this PackageArchiveReader package, NuGetFramework framework)
        {
            IEnumerable<string> GetFrameworkItems(IEnumerable<FrameworkSpecificGroup> items)
            {
                return items
                    .Where(item => item.TargetFramework.IsAny || item.TargetFramework == framework)
                    .SelectMany(item => item.Items);
            }

            var libs = GetFrameworkItems(package.GetLibItems());
            var build = GetFrameworkItems(package.GetBuildItems());
            var tools = GetFrameworkItems(package.GetToolItems());

            return libs.Concat(build).Concat(tools);
        }
    }
}
