using System;
using System.CommandLine;

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
            string feed = @"https://api.nuget.org/v3/index.json";

            var syntax = ArgumentSyntax.Parse(args, arg =>
            {
                arg.DefineOption("framework", ref framework, true, ".NET Target Framework Moniker");
                arg.DefineOption("name", ref name, true, "Name of NuGet package to download");
                arg.DefineOption("version", ref version, true, "Version of NuGet package");
                arg.DefineOption("feed", ref feed, false, "NuGet feed to use");
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

            return new DownloadOptions
            {
                Feed = feed,
                Framework = framework,
                Version = version,
                Name = name
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
