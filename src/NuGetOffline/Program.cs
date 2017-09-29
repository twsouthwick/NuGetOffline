using Autofac;
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

            builder.Register(ctx =>
            {
                var o = ctx.Resolve<DownloadOptions>();

                IFolder CreateFolder<T>()
                    where T : IFolder
                {
                    var factory = ctx.Resolve<Func<string, T>>();

                    return factory(o.OutputPath);
                }

                if (o.ZipResults)
                {
                    return CreateFolder<ZipArchiveFolder>();
                }
                else
                {
                    return CreateFolder<FileSystemFolder>();
                }
            })
            .Named<IFolder>(nameof(IFolder))
            .SingleInstance();

            builder.RegisterType<FileSystemFolder>();
            builder.RegisterType<ZipArchiveFolder>();
            builder.RegisterType<MsbuildFileBuilder>();

            builder.RegisterInstance(Console.Out)
                .As<TextWriter>();

            builder.RegisterType<TextWriterLogger>()
                .As<ILogger>()
                .As<NuGet.Common.ILogger>()
                .SingleInstance();

            builder.RegisterDecorator<IFolder>((ctx, folder) =>
            {
                var factory = ctx.Resolve<Func<IFolder, MsbuildFileBuilder>>();

                return factory(folder);
            }, nameof(IFolder))
            .SingleInstance();

            return builder.Build();
        }
    }
}
