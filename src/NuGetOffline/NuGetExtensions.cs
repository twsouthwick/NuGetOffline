using NuGet.Frameworks;
using NuGet.Packaging;
using System;
using System.Collections.Generic;
using System.IO;
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
        public static IEnumerable<(string path, ReferenceInfo referenceInfo)> GetFrameworkItems(this PackageArchiveReader package, NuGetFramework framework)
        {
            IEnumerable<(string, ReferenceInfo)> GetFrameworkItems(IEnumerable<FrameworkSpecificGroup> items, Func<ICollection<string>, ReferenceInfo> referenceInfo)
            {
                var frameworkItems = items
                    .Where(framework)
                    .SelectMany(item => item.Items)
                    .ToList();

                var names = new HashSet<string>(frameworkItems.Select(Path.GetFileName), StringComparer.OrdinalIgnoreCase);

                return frameworkItems
                    .Select(i => (i, referenceInfo(names)));
            }

            var libs = GetFrameworkItems(package.GetLibItems(), names => names.Contains("ensureRedirect.xml") ? ReferenceInfo.ReferenceWithRedirect : ReferenceInfo.Reference);
            var build = GetFrameworkItems(package.GetBuildItems(), _ => ReferenceInfo.None);
            var tools = GetFrameworkItems(package.GetToolItems(), _ => ReferenceInfo.None);

            return libs.Concat(build).Concat(tools);
        }

        /// <summary>
        /// Get dependencies from a package for a given framework
        /// </summary>
        /// <param name="package">Package to find dependencies</param>
        /// <param name="framework">Framework to search for</param>
        /// <returns>Filtered list of dependencies</returns>
        public static IEnumerable<PackageDependencyGroup> GetPackageDependencies(this PackageArchiveReader package, NuGetFramework framework)
        {
            return package.GetPackageDependencies()
                    .Where(framework);
        }

        private static IEnumerable<T> Where<T>(this IEnumerable<T> items, NuGetFramework framework)
            where T : IFrameworkSpecific
        {
            return items
                .Where(item => item.TargetFramework.IsAny || item.TargetFramework == framework);
        }
    }
}
