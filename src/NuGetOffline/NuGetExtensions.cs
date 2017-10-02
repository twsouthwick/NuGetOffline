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
        public static IEnumerable<(string path, bool isReference)> GetFrameworkItems(this PackageArchiveReader package, NuGetFramework framework)
        {
            IEnumerable<(string, bool)> GetFrameworkItems(IEnumerable<FrameworkSpecificGroup> items, bool isReference)
            {
                return items
                    .Where(item => item.TargetFramework.IsAny || item.TargetFramework == framework)
                    .SelectMany(item => item.Items)
                    .Select(i => (i, isReference));
            }

            var libs = GetFrameworkItems(package.GetLibItems(), true);
            var build = GetFrameworkItems(package.GetBuildItems(), false);
            var tools = GetFrameworkItems(package.GetToolItems(), false);

            return libs.Concat(build).Concat(tools);
        }
    }
}
