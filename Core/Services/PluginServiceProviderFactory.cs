using Anna.Core.Services.Extensions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Unity;

namespace Anna.Core.Services
{
    public static class PluginServiceProviderExtensions
    {
        public static IHostBuilder UsePluginServiceProvider(this IHostBuilder hostBuilder, IUnityContainer container = null)
        {
            var factory = new PluginServiceProviderFactory();

            return hostBuilder.UseServiceProviderFactory<IUnityContainer>(factory)
                              .ConfigureServices((context, services) =>
                              {
                                  services.Replace(ServiceDescriptor.Singleton<IServiceProviderFactory<IUnityContainer>>(factory));
                              });
        }

        public static IApplicationBuilder AlterEndpointRouting(this IApplicationBuilder builder)
        {
            var routeOptions = builder.ApplicationServices.GetRequiredService<IOptions<RouteOptions>>();
            var endpointDataSource = builder.ApplicationServices.GetRequiredService<EndpointDataSource>();

            // THis will not work since EndpointDataSources is internal
            //if(routeOptions.Value.EndpointDataSources == null)
            //{
            //    routeOptions.Value.EndpointDataSources = endpointDataSource;
            //}

            return builder;
        }
    }
    public class PluginServiceProviderFactory : IServiceProviderFactory<IUnityContainer>
    {

        IUnityContainer IServiceProviderFactory<IUnityContainer>.CreateBuilder(IServiceCollection services)
        {
            return CreateServiceProviderContainer(services);
        }

        public IServiceProvider CreateServiceProvider(IUnityContainer containerBuilder)
        {
            var SP = new PluginServiceProvider(containerBuilder);
            return SP;
        }

        private IUnityContainer CreateServiceProviderContainer(IServiceCollection services)
        {
            var container = new UnityContainer();
            return container.AddExtension(new PluginMdiExtension())
                            .AddServices(services);
        }

    }
}
