using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        private readonly IAssemblyNameFinder _assemblyNameFinder;

        private readonly List<(string name, string fullName)> _references = new List<(string, string)>();
        private readonly List<string> _targets = new List<string>();
        private readonly List<string> _props = new List<string>();
        private readonly List<(string name, AssemblyName assembly)> _redirects = new List<(string, AssemblyName)>();

        private static readonly XNamespace NS = "http://schemas.microsoft.com/developer/msbuild/2003";

        /// <summary>
        /// Initializes a new instance of the <see cref="MsbuildFileBuilder"/> class.
        /// </summary>
        /// <param name="other">Folder to delegate to</param>
        public MsbuildFileBuilder(IFolder other, ILogger logger, IAssemblyNameFinder assemblyNameFinder)
        {
            _other = other;
            _logger = logger;
            _assemblyNameFinder = assemblyNameFinder;
        }

        /// <inheritdoc/>
        public async Task AddAsync(string name, Stream stream, ReferenceInfo referenceInfo)
        {
            var bytes = GetEntryBytes(stream);
            var assemblyName = GetReferenceAssemblyName(name, bytes);

            _logger.Verbose($"Adding {assemblyName?.FullName ?? name}");

            switch (Path.GetExtension(name).ToUpperInvariant())
            {
                case ".TARGETS":
                    _targets.Add(name);
                    break;
                case ".PROPS":
                    _props.Add(name);
                    break;
                case ".DLL":
                    Debug.Assert(assemblyName != null);

                    if (referenceInfo == ReferenceInfo.Reference || referenceInfo == ReferenceInfo.ReferenceWithRedirect)
                    {
                        _references.Add((name, assemblyName.FullName));
                    }

                    if (referenceInfo == ReferenceInfo.ReferenceWithRedirect)
                    {
                        _redirects.Add((name, assemblyName));
                    }

                    break;
            }

            using (var ms = new MemoryStream(bytes))
            {
                await _other.AddAsync(name, ms, referenceInfo);
            }
        }

        /// <inheritdoc/>
        public async Task SaveAsync()
        {
            await AddDocumentAsync("NuGet.Imports.props", CreatePropsFile());
            await AddDocumentAsync("NuGet.Imports.targets", CreateTargetsFile());
            await AddDocumentAsync("App.config", CreateAppConfig());

            await _other.SaveAsync();

            _logger.Info("In order to add this to a project, please import the .props file at the top of the csproj/vbproj of interest, the .targets at bottom and import the binding redirects in App.config");
        }

        private async Task AddDocumentAsync(string name, XDocument document)
        {
            using (var ms = new MemoryStream())
            {
                document.Save(ms, SaveOptions.OmitDuplicateNamespaces);

                ms.Position = 0;

                await _other.AddAsync(name, ms, ReferenceInfo.None);
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

        private XDocument CreateAppConfig()
        {
            XNamespace NS = "urn:schemas-microsoft-com:asm.v1";
            XElement BuildDependency(AssemblyName name)
            {
                var culture = string.IsNullOrEmpty(name.CultureName) ? "neutral" : name.CultureName;
                var publicKeyToken = BitConverter.ToString(name.GetPublicKeyToken()).Replace("-", string.Empty).ToLowerInvariant();

                return new XElement(NS + "dependentAssembly",
                    new XElement(NS + "assemblyIdentity", new XAttribute("name", name.Name), new XAttribute("publicKeyToken", publicKeyToken), new XAttribute("culture", culture)),
                    new XElement(NS + "bindingRedirect", new XAttribute("oldVersion", $"0.0.0.0-{name.Version}"), new XAttribute("newVersion", name.Version)));
            }

            var redirects = _redirects.Select(r => BuildDependency(r.assembly));

            return new XDocument(new XElement("configuration",
                new XElement("runtime",
                new XElement(NS + "assemblyBinding", redirects))));
        }

        private static byte[] GetEntryBytes(Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);

                return ms.ToArray();
            }
        }

        private AssemblyName GetReferenceAssemblyName(string name, byte[] bytes)
        {
            if (!name.EndsWith(".dll", System.StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return _assemblyNameFinder.GetAssemblyName(bytes);
        }

        private static string GetPath(string path) => $@"$(MSBuildThisFileDirectory)\{path}";
    }
}
