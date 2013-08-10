using ENode.Eventing;
using ENode.Infrastructure;
using ENode.Messaging;

namespace ENode.Autofac {
    /// <summary>ENode configuration class Autofac extensions.
    /// </summary>
    public static class ConfigurationExtensions {
        /// <summary>Use Autofac as the object container for the enode framework.
        /// </summary>
        /// <returns></returns>
        public static Configuration UseAutofac(this Configuration configuration) {
            ObjectContainer.SetContainer(new AutofacObjectContainer());
            return configuration;
        }
    }
}