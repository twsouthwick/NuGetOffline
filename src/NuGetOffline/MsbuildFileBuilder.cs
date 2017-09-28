using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NuGetOffline
{
    /// <summary>
    /// An implementation of <see cref="IFolder"/> that generates an msbuild file
    /// </summary>
    internal class MsbuildFileBuilder : IFolder
    {
        private readonly IFolder _other;

        private readonly List<string> _references = new List<string>();
        private readonly List<string> _targets = new List<string>();
        private readonly List<string> _props = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MsbuildFileBuilder"/> class.
        /// </summary>
        /// <param name="other">Folder to delegate to</param>
        public MsbuildFileBuilder(IFolder other)
        {
            _other = other;
        }

        /// <inheritdoc/>
        public Task AddAsync(string name, Stream stream)
        {
            switch (Path.GetExtension(name).ToUpperInvariant())
            {
                case ".TARGETS":
                    _targets.Add(name);
                    break;
                case ".PROPS":
                    _props.Add(name);
                    break;
                case ".DLL":
                    _references.Add(name);
                    break;
            }

            return _other.AddAsync(name, stream);
        }

        /// <inheritdoc/>
        public Task SaveAsync()
        {
            return Task.CompletedTask;
        }
    }
}
