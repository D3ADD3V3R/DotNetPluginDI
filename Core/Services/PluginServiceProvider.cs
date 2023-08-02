using System.Diagnostics.CodeAnalysis;
using Anna.Core.Helper;
using Anna.Core.Services.Extensions;
using Unity;
using Unity.Lifetime;

namespace Anna.Core.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class PluginServiceProvider : IPluginServiceProvider,
                                   ISupportRequiredService,
                                   IServiceScopeFactory,
                                   IServiceScope,
                                   IDisposable
    {

        private IUnityContainer _container;
        private ILogger<PluginServiceProvider> _logger;
        private Dictionary<string, IUnityContainer> _childProviders;

#if DEBUG
        private string id = Guid.NewGuid().ToString();
#endif


        internal PluginServiceProvider(IUnityContainer container)
        {
            _container = container;
            _container.RegisterInstance<IServiceScope>(this, new ExternallyControlledLifetimeManager());
            _container.RegisterInstance<IServiceProvider>(this, new PluginServiceProviderLifetimeManager(this));
            _container.RegisterInstance<IServiceScopeFactory>(this, new ExternallyControlledLifetimeManager());
            _logger = _container.Resolve<ILogger<PluginServiceProvider>>();
            _childProviders = new Dictionary<string, IUnityContainer>();
        }


        /// <summary>
        ///     Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>A service object of type serviceType. -or- null if there is no service object of type serviceType.</returns>
        #region IServiceProvider

        public object GetService(Type serviceType)
        {
            _logger.LogDebug("GetService getriggered => {ser}", serviceType);

            if (null == _container)
                throw new ObjectDisposedException(nameof(IServiceProvider));

            try
            {
                return _container.Resolve(serviceType, null);
            }
            catch { /* Ignore */}

            return null;
        }

        public object GetRequiredService(Type serviceType)
        {
            _logger.LogDebug("GetRequiredService getriggered => {ser}", serviceType);
            if (null == _container)
                throw new ObjectDisposedException(nameof(IServiceProvider));

            return _container.Resolve(serviceType, null);
        }


        public IPluginServiceProvider RegisterService<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(ServiceLifetime lifetime)
        {
            return RegisterService(typeof(TService), typeof(TImplementation), lifetime);
        }

        public IPluginServiceProvider RegisterService(Type serviceType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType, ServiceLifetime lifetime)
        {
            AnnaThrowHelper.ThrowIfNull(serviceType);
            AnnaThrowHelper.ThrowIfNull(implementationType);
            AnnaThrowHelper.ThrowIfNull(lifetime);

            ServiceDescriptor newService = new ServiceDescriptor(serviceType, implementationType, lifetime);

            return RegisterService(newService);
        }

        public IPluginServiceProvider RegisterService(ServiceDescriptor serviceDescriptor)
        {
            var lifetime = _container.Resolve<PluginMdiExtension>().Lifetime;



            _container.Register(serviceDescriptor, lifetime);
            return this;
        }

        public void RegisterChildProvider(string identifier, IServiceCollection serviceContainer)
        {
            _logger.LogDebug("Adding Child-SerivceProvider: [{name}]", identifier);
            IUnityContainer newContainer = _container.CreateChildContainer();
            newContainer.AddServices(serviceContainer);
            _childProviders.Add(identifier, newContainer);
        }

        #endregion



        #region IServiceScopeFactory

        public IServiceScope CreateScope()
        {
            return new PluginServiceProvider(_container.CreateChildContainer());
        }

        #endregion

        #region IServiceScope

        IServiceProvider IServiceScope.ServiceProvider => this;

        #endregion

        #region Public Members

        public static IPluginServiceProvider ConfigureServices(IServiceCollection services)
        {
            return new PluginServiceProvider(new UnityContainer()
                .AddExtension(new PluginMdiExtension())
                .AddServices(services));

        }

        public static explicit operator UnityContainer(PluginServiceProvider c)
        {
            return (UnityContainer)c._container;
        }

        #endregion

        #region Disposable

        public void DisposeChild(string ident)
        {
            _childProviders[ident].Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            IDisposable disposable = _container;

            foreach (var provider in _childProviders)
            {
                (provider.Value).Dispose();
            }
            _childProviders.Clear();

            _container = null;
            disposable?.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }
}
