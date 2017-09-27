using Autofac;
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
            using (var container = CreateContainer())
            {
                await container.Resolve<NuGetOfflineDownloader>().RunAsync();
            }
        }

        private static IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<NuGetOfflineDownloader>()
                .AsSelf()
                .InstancePerLifetimeScope();

            return builder.Build();
        }
    }
}
