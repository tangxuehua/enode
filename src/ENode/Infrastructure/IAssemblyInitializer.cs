using System.Reflection;

namespace ENode.Infrastructure
{
    /// <summary>Represents a initializer which can initialize from the given assemblies.
    /// </summary>
    public interface IAssemblyInitializer
    {
        /// <summary>Initialize from the given assemblies.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        void Initialize(params Assembly[] assemblies);
    }
}
