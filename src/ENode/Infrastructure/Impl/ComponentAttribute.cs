using System;

namespace ENode.Infrastructure {
    /// <summary>An attribute to indicate a class is a component.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ComponentAttribute : Attribute {
        public LifeStyle LifeStyle { get; private set; }
        public ComponentAttribute() : this(LifeStyle.Transient) { }
        public ComponentAttribute(LifeStyle lifeStyle) {
            LifeStyle = lifeStyle;
        }
    }
    /// <summary>An enum to description the lifetime of a component.
    /// </summary>
    public enum LifeStyle {
        Transient,
        Singleton
    }
}
