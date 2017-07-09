

namespace Umizoo.Configurations
{
    using System.Configuration;
    using Microsoft.Practices.ServiceLocation;
    using Microsoft.Practices.Unity;
    using Microsoft.Practices.Unity.Configuration;
    using Umizoo.Infrastructure.Composition;

    public static class ConfigurationExtentions
    {
        public static void DoneWithUnity(this Configuration that)
        {
            that.DoneWithUnity(new UnityContainer());
        }
        
        private static void DoneWithUnity(this Configuration that, IUnityContainer container)
        {
            Ensure.NotNull(container, "container");
            ServiceLocator.SetLocatorProvider(() => new UnityServiceLocator(container));

            that.Done(new UnityObjectContainer(container));
        }

        public static void DoneWithUnityByConfig(this Configuration that, string sectionName)
        {
            Ensure.NotNullOrWhiteSpace(sectionName, "sectionName");

            var section = ConfigurationManager.GetSection(sectionName) as UnityConfigurationSection;
            var container = section.Configure(new UnityContainer());

            that.DoneWithUnity(container);
        } 
    }
}
