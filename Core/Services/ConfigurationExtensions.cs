﻿using System.Reflection;
using Anna.Core.Services.Extensions;
using Anna.Core.Services.Lifetime;
using Unity;
using Unity.Lifetime;
using Unity.Microsoft.DependencyInjection.Lifetime;

namespace Anna.Core.Services
{
    internal static class ConfigurationExtensions
    {
        internal static IUnityContainer AddServices(this IUnityContainer container, IServiceCollection services)
        {
            var lifetime = ((UnityContainer)container).Configure<PluginMdiExtension>().Lifetime;
            //var registerFunc = ((UnityContainer)container).Register;

            //((UnityContainer)container).Register = ((UnityContainer)container).AppendNew;

            foreach (var descriptor in services) container.Register(descriptor, lifetime);

            //((UnityContainer)container).Register = registerFunc;

            return container;
        }


        internal static void Register(this IUnityContainer container,
            ServiceDescriptor serviceDescriptor, ILifetimeContainer lifetime)
        {
            if (serviceDescriptor.ImplementationType != null)
            {
                var name = serviceDescriptor.ServiceType.IsGenericTypeDefinition ? UnityContainer.All : null;
                container.RegisterType(serviceDescriptor.ServiceType,
                                       serviceDescriptor.ImplementationType,
                                       name,
                                       (ITypeLifetimeManager)serviceDescriptor.GetLifetime(lifetime));
            }
            else if (serviceDescriptor.ImplementationFactory != null)
            {
                container.RegisterFactory(serviceDescriptor.ServiceType,
                                        null,
                                        scope =>
                                        {
                                            var serviceProvider = scope.Resolve<IServiceProvider>();
                                            var instance = serviceDescriptor.ImplementationFactory(serviceProvider);
                                            return instance;
                                        },
                                       (IFactoryLifetimeManager)serviceDescriptor.GetLifetime(lifetime));
            }
            else if (serviceDescriptor.ImplementationInstance != null)
            {
                container.RegisterInstance(serviceDescriptor.ServiceType,
                                           null,
                                           serviceDescriptor.ImplementationInstance,
                                           (IInstanceLifetimeManager)serviceDescriptor.GetLifetime(lifetime));
            }
            else
            {
                throw new InvalidOperationException("Unsupported registration type");
            }
        }


        internal static LifetimeManager GetLifetime(this ServiceDescriptor serviceDescriptor, ILifetimeContainer lifetime)
        {
            switch (serviceDescriptor.Lifetime)
            {
                case ServiceLifetime.Scoped:
                    return new HierarchicalLifetimeManager();
                case ServiceLifetime.Singleton:
                    return new InjectionSingletonLifetimeManager(lifetime);
                case ServiceLifetime.Transient:
                    return new AnnaInjectionTransientLifetimeManager();
                default:
                    throw new NotImplementedException(
                        $"Unsupported lifetime manager type '{serviceDescriptor.Lifetime}'");
            }
        }


        internal static bool CanResolve(this IUnityContainer container, Type type)
        {
            var info = type.GetTypeInfo();

            if (info.IsClass && !info.IsAbstract)
            {
                if (typeof(Delegate).GetTypeInfo().IsAssignableFrom(info) || typeof(string) == type || info.IsEnum
                    || type.IsArray || info.IsPrimitive)
                {
                    return container.IsRegistered(type);
                }
                return true;
            }

            if (info.IsGenericType)
            {
                var gerericType = type.GetGenericTypeDefinition();
                if ((gerericType == typeof(IEnumerable<>)) ||
                    container.IsRegistered(gerericType))
                {
                    return true;
                }
            }

            return container.IsRegistered(type);
        }
    }
}