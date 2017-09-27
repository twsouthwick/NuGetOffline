using Autofac;
using System;
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
                    await container.Resolve<NuGetOfflineDownloader>().RunAsync();
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

            builder.RegisterType<NuGetOfflineDownloader>()
                .AsSelf()
                .InstancePerLifetimeScope();

            return builder.Build();
        }
    }
}
