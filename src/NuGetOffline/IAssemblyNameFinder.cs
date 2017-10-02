using System.Reflection;

namespace NuGetOffline
{
    /// <summary>
    /// A service to help retrieve the assembly name of a byte stream
    /// </summary>
    public interface IAssemblyNameFinder
    {
        /// <summary>
        /// Gets the assembly name of the assembly contained in the bytes
        /// </summary>
        /// <param name="bytes">The assembly bytes</param>
        /// <returns>The assembly name</returns>
        AssemblyName GetAssemblyName(byte[] bytes);
    }
}
