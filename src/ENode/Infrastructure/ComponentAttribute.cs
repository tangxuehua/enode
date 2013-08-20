using System;

namespace ENode.Infrastructure
{
    /// <summary>An attribute to indicate a class is a component.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ComponentAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public LifeStyle LifeStyle { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public ComponentAttribute() : this(LifeStyle.Transient) { }
        /// <summary>
        /// 
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
        /// <summary>
        /// 
        /// </summary>
        Transient,
        /// <summary>
        /// 
        /// </summary>
        Singleton
    }
}
