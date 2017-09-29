using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace NuGetOffline
{
    /// <summary>
    /// An implementation of <see cref="IFolder"/> that will add to zip file
    /// </summary>
    internal class ZipArchiveFolder : IFolder
    {
        private readonly ZipArchive _archive;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZipArchiveFolder"/> class.
        /// </summary>
        /// <param name="path">Path to write file</param>
        public ZipArchiveFolder(string path)
        {
            _archive = new ZipArchive(File.OpenWrite(path), ZipArchiveMode.Create);
        }

        /// <inheritdoc/>
        public async Task AddAsync(string name, Stream stream)
        {
            using (var entry = _archive.CreateEntry(name).Open())
            {
                await stream.CopyToAsync(entry);
            }
        }

        /// <inheritdoc/>
        public Task SaveAsync()
        {
            _archive.Dispose();

            return Task.CompletedTask;
        }
    }
}
