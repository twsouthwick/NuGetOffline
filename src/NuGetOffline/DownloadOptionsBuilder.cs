using System;
using System.CommandLine;
using System.IO;

namespace NuGetOffline
{
    /// <summary>
    /// Class to build instances of <see cref="DownloadOptions"/>
    /// </summary>
    public static class DownloadOptionsBuilder
    {
        /// <summary>
        /// Parse string arguments to generate download options
        /// </summary>
        /// <param name="args">The command line arguments</param>
        /// <returns>The options defined by the command line arguments</returns>
        public static DownloadOptions Parse(string[] args)
        {
            string framework = null;
            string version = null;
            string name = null;
            string path = null;
            var feed = @"https://api.nuget.org/v3/index.json";
            var zip = false;

            var syntax = ArgumentSyntax.Parse(args, arg =>
            {
                arg.DefineOption("framework", ref framework, true, ".NET Target Framework Moniker");
                arg.DefineOption("name", ref name, true, "Name of NuGet package to download");
                arg.DefineOption("version", ref version, true, "Version of NuGet package");
                arg.DefineOption("feed", ref feed, false, "NuGet feed to use");
                arg.DefineOption("zip", ref zip, false, "Zip results");
                arg.DefineOption("path", ref path, true, "Path to write output to");
            });

            if (string.IsNullOrWhiteSpace(framework))
            {
                throw new ArgumentParsingException(syntax, "Must supply a framework");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentParsingException(syntax, "Must supply a NuGet package name");
            }

            if (string.IsNullOrWhiteSpace(version))
            {
                throw new ArgumentParsingException(syntax, "Must supply a NuGet package version");
            }

            if (string.IsNullOrWhiteSpace(feed))
            {
                throw new ArgumentParsingException(syntax, "Must supply a NuGet package feed");
            }

#if !DEBUG
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentParsingException(syntax, "Must supply a path");
            }
#endif

            if (File.Exists(path) || Directory.Exists(path))
            {
                throw new ArgumentParsingException(syntax, "Path already exists");
            }

            if (zip && !string.Equals(Path.GetExtension(path), ".zip", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentParsingException(syntax, "Path must end in '.zip' if a zip file is to be created");
            }

            return new DownloadOptions
            {
                Feed = feed,
                Framework = framework,
                Name = name,
                OutputPath = path,
                Version = version,
                ZipResults = zip,
            };
        }

        private class ArgumentParsingException : NuGetDownloaderException
        {
            private readonly ArgumentSyntax _syntax;

            /// <summary>
            /// Initializes a new instance of the <see cref="ArgumentParsingException"/> class.
            /// </summary>
            /// <param name="syntax">The argument syntax used to parse the command line</param>
            /// <param name="message">Exception message</param>
            public ArgumentParsingException(ArgumentSyntax syntax, string message)
                : base(message)
            {
                _syntax = syntax;
            }

            public override string Message => $"{base.Message}{Environment.NewLine}{Environment.NewLine}{_syntax.GetHelpText()}";
        }
    }
}
