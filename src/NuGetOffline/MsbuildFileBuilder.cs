using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;

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

        private static readonly XNamespace NS = "http://schemas.microsoft.com/developer/msbuild/2003";

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
        public async Task SaveAsync()
        {
            await AddDocument("NuGet.Imports.props", CreatePropsFile());
            await AddDocument("NuGet.Imports.targets", CreateTargetsFile());

            await _other.SaveAsync();
        }

        private async Task AddDocument(string name, XDocument document)
        {
            using (var ms = new MemoryStream())
            {
                document.Save(ms, SaveOptions.OmitDuplicateNamespaces);

                ms.Position = 0;

                await _other.AddAsync(name, ms);
            }
        }

        private XDocument CreatePropsFile()
        {
            var element = new XElement(NS + "Project", new XAttribute(NS + "ToolsVersion", "12.0"));

            foreach (var prop in _props)
            {
                element.Add(new XElement(NS + "Import", new XAttribute(NS + "Project", GetPath(prop))));
            }

            return new XDocument(element);
        }

        private XDocument CreateTargetsFile()
        {
            var element = new XElement(NS + "Project", new XAttribute(NS + "ToolsVersion", "12.0"));

            var itemGroup = new XElement(NS + "ItemGroup");

            foreach (var reference in _references)
            {
                itemGroup.Add(new XElement(NS + "Reference", new XAttribute(NS + "Include", GetPath(reference))));
            }

            element.Add(itemGroup);

            foreach (var target in _targets)
            {
                element.Add(new XElement(NS + "Import", new XAttribute(NS + "Project", GetPath(target))));
            }

            return new XDocument(element);
        }

        private static string GetPath(string path) => $@"$(MSBuildThisFileDirectory)\{path}";
    }
}
