using Autofac;
using NuGet.Common;
using NuGet.Protocol.Core.Types;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NuGetOffline
{
    /// <summary>
    /// Entry point for applications
    /// </summary>
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            try
            {
                var options = DownloadOptionsBuilder.Parse(args);

                using (var container = CreateContainer(options))
                {
                    var folder = container.Resolve<IFolder>();

                    await container.Resolve<NuGetOfflineDownloader>().RunAsync(options, folder, CancellationToken.None);

                    await folder.SaveAsync();
                }
            }
            catch (NuGetDownloaderException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static IContainer CreateContainer(DownloadOptions options)
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(options);

            builder.RegisterType<SourceCacheContext>()
                .SingleInstance();

            builder.RegisterType<NuGetOfflineDownloader>()
                 .AsSelf()
                 .InstancePerLifetimeScope();

            builder.RegisterInstance(Console.Out)
                .As<TextWriter>();

            builder.RegisterType<NuGetTextWriterLogger>()
                .As<ILogger>()
                .SingleInstance();

            builder.RegisterType<FileSystemFolder>()
                .Named<IFolder>(nameof(IFolder))
                .SingleInstance();

            builder.RegisterDecorator<IFolder>(t => new MsbuildFileBuilder(t), nameof(IFolder))
                .SingleInstance();

            return builder.Build();
        }
    }
}
