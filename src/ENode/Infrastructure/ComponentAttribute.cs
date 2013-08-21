using System;

namespace ENode.Infrastructure
{
    /// <summary>An attribute to indicate a class is a component.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ComponentAttribute : Attribute
    {
        /// <summary>The lifetime of the component.
        /// </summary>
        public LifeStyle LifeStyle { get; private set; }
        /// <summary>Default constructor.
        /// </summary>
        public ComponentAttribute() : this(LifeStyle.Transient) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="lifeStyle"></param>
        public ComponentAttribute(LifeStyle lifeStyle)
        {
            LifeStyle = lifeStyle;
        }
    }
    /// <summary>An enum to description the lifetime of a component.
    /// </summary>
    public enum LifeStyle
    {
        /// <summary>Represents a component is a transient component.
        /// </summary>
        Transient,
        /// <summary>Represents a component is a singleton component.
        /// </summary>
        Singleton
    }
}
