using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ENode.Infrastructure
{
    public class ObjectHelper
    {
        /// <summary>创建一个T类型的对象实例，自动将source对象中相同名称的属性更新到该新创建的实例。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T CreateObject<T>(object source) where T : class, new()
        {
            var obj = new T();
            var propertiesFromSource = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var property in properties)
            {
                var sourceProperty = propertiesFromSource.FirstOrDefault(x => x.Name == property.Name);
                if (sourceProperty != null)
                {
                    property.SetValue(obj, sourceProperty.GetValue(source, null), null);
                }
            }

            return obj;
        }
        /// <summary>将source对象中指定的属性更新到target对象中
        /// </summary>
        /// <typeparam name="TTarget">target对象的类型</typeparam>
        /// <typeparam name="TSource">source对象的类型</typeparam>
        /// <param name="target">target对象</param>
        /// <param name="source">source对象</param>
        /// <param name="propertyExpressionsFromSource">source对象中需要更新到target对象的属性</param>
        public static void UpdateObject<TTarget, TSource>(TTarget target, TSource source, params Expression<Func<TSource, object>>[] propertyExpressionsFromSource)
            where TTarget : class
            where TSource : class
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (propertyExpressionsFromSource == null)
            {
                throw new ArgumentNullException("propertyExpressionsFromSource");
            }

            var properties = target.GetType().GetProperties();

            foreach (var propertyExpression in propertyExpressionsFromSource)
            {
                var propertyFromSource = GetProperty<TSource, object>(propertyExpression);
                var propertyFromTarget = properties.SingleOrDefault(x => x.Name == propertyFromSource.Name);
                if (propertyFromTarget != null)
                {
                    propertyFromTarget.SetValue(target, propertyFromSource.GetValue(source, null), null);
                }
            }
        }

        private static PropertyInfo GetProperty<TSource, TProperty>(Expression<Func<TSource, TProperty>> lambda)
        {
            Type type = typeof(TSource);
            MemberExpression memberExpression = null;

            if (lambda.Body.NodeType == ExpressionType.Convert)
            {
                memberExpression = ((UnaryExpression)lambda.Body).Operand as MemberExpression;
            }
            else if (lambda.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExpression = lambda.Body as MemberExpression;
            }

            if (memberExpression == null)
            {
                throw new ArgumentException(string.Format("Invalid Lambda Expression '{0}'.", lambda.ToString()));
            }

            PropertyInfo propInfo = memberExpression.Member as PropertyInfo;
            if (propInfo == null)
            {
                throw new ArgumentException(string.Format("Expression '{0}' refers to a field, not a property.", lambda.ToString()));
            }

            if (type != propInfo.ReflectedType && !type.IsSubclassOf(propInfo.ReflectedType))
            {
                throw new ArgumentException(string.Format("Expresion '{0}' refers to a property that is not from type {1}.", lambda.ToString(), type));
            }

            return propInfo;
        }
    }
}