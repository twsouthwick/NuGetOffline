using System;
using System.IO;
using System.Threading.Tasks;

namespace NuGetOffline
{
    /// <summary>
    /// An implementation of <see cref="IFolder"/> that uses the filesystem
    /// </summary>
    public class FileSystemFolder : IFolder
    {
        private readonly string _path;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemFolder"/> class.
        /// </summary>
        public FileSystemFolder()
        {
            _path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            Directory.CreateDirectory(_path);

            Console.WriteLine($"Using temp folder: {_path}");
        }

        /// <inheritdoc/>
        public async Task AddAsync(string name, Stream stream)
        {
            var path = new FileInfo(Path.Combine(_path, name));

            if (path.Exists)
            {
                throw new InvalidOperationException($"File {name} already exists");
            }

            if (!path.Directory.Exists)
            {
                path.Directory.Create();
            }

            using (var fs = path.OpenWrite())
            {
                await stream.CopyToAsync(fs);
            }
        }

        /// <inheritdoc/>
        public Task SaveAsync() => Task.CompletedTask;
    }
}
