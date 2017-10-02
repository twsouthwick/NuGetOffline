using System.IO;
using System.Threading.Tasks;

namespace NuGetOffline
{
    /// <summary>
    /// An abstract for folders for results to be written to
    /// </summary>
    public interface IFolder
    {
        /// <summary>
        /// Add an item to the folder
        /// </summary>
        /// <param name="name">The name of the item</param>
        /// <param name="stream">A stream to be written</param>
        /// <param name="referenceInfo">Metadata as to how to reference item from the lib folder</param>
        Task AddAsync(string name, Stream stream, ReferenceInfo referenceInfo);

        /// <summary>
        /// Completes the folder generation
        /// </summary>
        Task SaveAsync();
    }
}
