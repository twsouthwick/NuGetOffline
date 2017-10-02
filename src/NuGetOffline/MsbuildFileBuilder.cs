﻿using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
        private readonly ILogger _logger;
        private readonly List<(string name, string fullName)> _references = new List<(string, string)>();
        private readonly List<string> _targets = new List<string>();
        private readonly List<string> _props = new List<string>();

        private static readonly XNamespace NS = "http://schemas.microsoft.com/developer/msbuild/2003";

        /// <summary>
        /// Initializes a new instance of the <see cref="MsbuildFileBuilder"/> class.
        /// </summary>
        /// <param name="other">Folder to delegate to</param>
        public MsbuildFileBuilder(IFolder other, ILogger logger)
        {
            _other = other;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task AddAsync(string name, Stream stream, bool isReference)
        {
            var bytes = GetEntryBytes(stream);
            var fullName = GetReferenceName(name, bytes);

            _logger.Verbose($"Adding {fullName}");

            switch (Path.GetExtension(name).ToUpperInvariant())
            {
                case ".TARGETS":
                    _targets.Add(name);
                    break;
                case ".PROPS":
                    _props.Add(name);
                    break;
                case ".DLL":
                    if (isReference)
                    {
                        _references.Add((name, fullName));
                    }

                    break;
            }

            using (var ms = new MemoryStream(bytes))
            {
                await _other.AddAsync(name, ms, isReference);
            }
        }

        /// <inheritdoc/>
        public async Task SaveAsync()
        {
            await AddDocument("NuGet.Imports.props", CreatePropsFile());
            await AddDocument("NuGet.Imports.targets", CreateTargetsFile());

            await _other.SaveAsync();

            _logger.Info("In order to add this to a project, please import the .props file at the top of the csproj/vbproj of interest, the .targets at bottom");
        }

        private async Task AddDocument(string name, XDocument document)
        {
            using (var ms = new MemoryStream())
            {
                document.Save(ms, SaveOptions.OmitDuplicateNamespaces);

                ms.Position = 0;

                await _other.AddAsync(name, ms, false);
            }
        }

        private XDocument CreatePropsFile()
        {
            var element = new XElement(NS + "Project", new XAttribute("ToolsVersion", "12.0"));

            foreach (var prop in _props)
            {
                element.Add(new XElement(NS + "Import", new XAttribute("Project", GetPath(prop))));
            }

            return new XDocument(element);
        }

        private XDocument CreateTargetsFile()
        {
            var element = new XElement(NS + "Project", new XAttribute("ToolsVersion", "12.0"));

            var itemGroup = new XElement(NS + "ItemGroup");

            foreach (var reference in _references)
            {
                var referenceElement = new XElement(NS + "Reference",
                    new XAttribute("Include", reference.fullName),
                    new XElement(NS + "HintPath", GetPath(reference.name)));

                itemGroup.Add(referenceElement);
            }

            element.Add(itemGroup);

            foreach (var target in _targets)
            {
                element.Add(new XElement(NS + "Import", new XAttribute("Project", GetPath(target))));
            }

            return new XDocument(element);
        }

        private static byte[] GetEntryBytes(Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);

                return ms.ToArray();
            }
        }

        private static string GetReferenceName(string name, byte[] bytes)
        {
            if (!name.EndsWith(".dll", System.StringComparison.OrdinalIgnoreCase))
            {
                return name;
            }

            var assembly = Assembly.ReflectionOnlyLoad(bytes);
            return assembly.FullName;
        }

        private static string GetPath(string path) => $@"$(MSBuildThisFileDirectory)\{path}";
    }
}
