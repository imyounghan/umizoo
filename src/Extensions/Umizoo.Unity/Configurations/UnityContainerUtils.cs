using System.Configuration;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Umizoo.Infrastructure;
using Umizoo.Infrastructure.Composition;

namespace Umizoo.Configurations
{
    public static class UnityContainerUtils
    {
        public static IObjectContainer Build()
        {
            return Build(new UnityContainer());
        }

        public static IObjectContainer Build(IUnityContainer container)
        {
            Assertions.NotNull(container, "container");
            ServiceLocator.SetLocatorProvider(() => new UnityServiceLocator(container));

            return new UnityObjectContainer(container);
        }

        public static IObjectContainer Build(string sectionName)
        {
            Assertions.NotNullOrWhiteSpace(sectionName, "sectionName");

            var section = ConfigurationManager.GetSection(sectionName) as UnityConfigurationSection;
            var container = section.Configure(new UnityContainer());

            return Build(container);
        }
    }
}