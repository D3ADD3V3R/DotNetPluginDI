using Unity.Lifetime;

namespace Anna.Core.Services
{
    internal class PluginServiceProviderLifetimeManager : LifetimeManager, IInstanceLifetimeManager
    {
        private IServiceProvider _serviceProvider;

        public PluginServiceProviderLifetimeManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override object GetValue(ILifetimeContainer container = null)
        {
            return _serviceProvider;
        }

        protected override LifetimeManager OnCreateLifetimeManager()
        {
            throw new NotImplementedException();
        }
    }
}
