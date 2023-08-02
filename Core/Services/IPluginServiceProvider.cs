using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Unity;

namespace Anna.Core.Services
{
    /// <summary>
    ///     Defines a mechanism for retrieving a service object; that is, an object that
    ///     provides custom support to other objects.<br />
    ///     Specialized for the A.N.N.A. Plugin Engine
    /// </summary>
    public interface IPluginServiceProvider : IServiceProvider
    {
        public IPluginServiceProvider RegisterService<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(ServiceLifetime lifetime);
        public IPluginServiceProvider RegisterService(Type TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type TImplementation, ServiceLifetime lifetime);
        public IPluginServiceProvider RegisterService(ServiceDescriptor serviceDescriptor);
        public void RegisterChildProvider(string identifier, IServiceCollection serviceProvider);
        public void DisposeChild(string ident);
    }
}
