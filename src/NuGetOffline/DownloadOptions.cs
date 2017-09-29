namespace NuGetOffline
{
    /// <summary>
    /// Options to be used while downloading
    /// </summary>
    public class DownloadOptions
    {
        /// <summary>
        /// The NuGet feed to use
        /// </summary>
        public string Feed { get; set; }

        /// <summary>
        /// Gets or sets the framework that is being targeted
        /// </summary>
        public string Framework { get; set; }

        /// <summary>
        /// Gets or sets the version of the NuGet package desired
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the name of the NuGet package desired
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the path to write the files to
        /// </summary>
        public string OutputPath { get; set; }

        /// <summary>
        /// Gets or sets whether to zip results
        /// </summary>
        public bool ZipResults { get; set; }

        /// <summary>
        /// Gets or sets whether output is verbose
        /// </summary>
        public bool Verbose { get; set; }
    }
}
