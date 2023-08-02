using Unity.Extension;
using Unity.Lifetime;

namespace Anna.Core.Services.Extensions
{
    internal class PluginMdiExtension : UnityContainerExtension
    {
        protected override void Initialize()
        {
        }

        public ILifetimeContainer Lifetime => Context.Lifetime;
    }
}
