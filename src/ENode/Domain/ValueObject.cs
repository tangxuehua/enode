using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ENode.Domain
{
    /// <summary>
    /// 提供值对象基类，重写方法Equals，GetHashCode，以及重载==，!=这两个操作符
    /// 确保两个对象比较的时候是比较值而不是比较引用地址，
    /// 此基类支持值对象嵌套进行比较，用户在设计值对象时可以继承此类，从而减少很多值对象之间比较的工作
    /// </summary>
    [Serializable]
    public abstract class ValueObject<T> where T : class
    {
        /// <summary>
        /// 返回当前值对象类型具有哪些原子属性
        /// </summary>
        public abstract IEnumerable<object> GetAtomicValues();
        /// <summary>
        /// 返回当前值对象实例的深拷贝克隆实例
        /// </summary>
        /// <param name="objectContainsNewValues">包含新属性值的匿名对象</param>
        /// <returns>返回克隆后的新对象</returns>
        public T Clone(object objectContainsNewValues = null)
        {
            var propertyInfos = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            PropertyInfo[] newPropertyInfoArray = objectContainsNewValues != null ? objectContainsNewValues.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly) : null;
            var cloneObject = typeof(T).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null).Invoke(null) as T;

            if (newPropertyInfoArray != null)
            {
                foreach (var propertyInfo in propertyInfos)
                {
                    var property = newPropertyInfoArray.FirstOrDefault(x => x.Name == propertyInfo.Name);
                    if (property != null)
                    {
                        propertyInfo.SetValue(cloneObject, property.GetValue(objectContainsNewValues, null), null);
                    }
                    else
                    {
                        propertyInfo.SetValue(cloneObject, propertyInfo.GetValue(this, null), null);
                    }
                }
            }
            else
            {
                foreach (var propertyInfo in propertyInfos)
                {
                    propertyInfo.SetValue(cloneObject, propertyInfo.GetValue(this, null), null);
                }
            }

            return cloneObject;
        }

        /// <summary>
        /// 等于号操作符重载，将左右两个对象比较的是值，而不是对象引用地址
        /// </summary>
        public static bool operator ==(ValueObject<T> left, ValueObject<T> right)
        {
            return IsEqual(left, right);
        }
        /// <summary>
        /// 不等于号操作符重载，将左右两个对象比较的是值，而不是对象引用地址
        /// </summary>
        public static bool operator !=(ValueObject<T> left, ValueObject<T> right)
        {
            return !IsEqual(left, right);
        }
        /// <summary>
        /// 重写Equals方法，可以支持递归的方式比较两个对象的值是否完全相等
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            ValueObject<T> other = (ValueObject<T>)obj;
            IEnumerator<object> enumerator1 = this.GetAtomicValues().GetEnumerator();
            IEnumerator<object> enumerator2 = other.GetAtomicValues().GetEnumerator();
            bool enumerator1HasNextValue = enumerator1.MoveNext();
            bool enumerator2HasNextValue = enumerator2.MoveNext();

            while (enumerator1HasNextValue && enumerator2HasNextValue)
            {
                if (ReferenceEquals(enumerator1.Current, null) ^ ReferenceEquals(enumerator2.Current, null))
                {
                    return false;
                }
                if (enumerator1.Current != null)
                {
                    if (enumerator1.Current is IList && enumerator2.Current is IList)
                    {
                        if (!CompareEnumerables(enumerator1.Current as IList, enumerator2.Current as IList))
                        {
                            return false;
                        }
                    }
                    else if (!enumerator1.Current.Equals(enumerator2.Current))
                    {
                        return false;
                    }
                }
                enumerator1HasNextValue = enumerator1.MoveNext();
                enumerator2HasNextValue = enumerator2.MoveNext();
            }

            return !enumerator1HasNextValue && !enumerator2HasNextValue;
        }
        /// <summary>
        /// 重写GetHashCode，返回根据值得到的HashCode
        /// </summary>
        public override int GetHashCode()
        {
            return GetAtomicValues().Select(x => x != null ? x.GetHashCode() : 0).Aggregate((x, y) => x ^ y);
        }

        private static bool IsEqual(ValueObject<T> left, ValueObject<T> right)
        {
            if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
            {
                return false;
            }
            return ReferenceEquals(left, null) || left.Equals(right);
        }
        private static bool CompareEnumerables(IList enumerable1, IList enumerable2)
        {
            IEnumerator enumerator1 = enumerable1.GetEnumerator();
            IEnumerator enumerator2 = enumerable2.GetEnumerator();
            bool enumerator1HasNextValue = enumerator1.MoveNext();
            bool enumerator2HasNextValue = enumerator2.MoveNext();

            while (enumerator1HasNextValue && enumerator2HasNextValue)
            {
                if (ReferenceEquals(enumerator1.Current, null) ^ ReferenceEquals(enumerator2.Current, null))
                {
                    return false;
                }
                if (enumerator1.Current != null && enumerator2.Current != null)
                {
                    if (enumerator1.Current is IList && enumerator2.Current is IList)
                    {
                        if (!CompareEnumerables(enumerator1.Current as IList, enumerator2.Current as IList))
                        {
                            return false;
                        }
                    }
                    else if (!enumerator1.Current.Equals(enumerator2.Current))
                    {
                        return false;
                    }
                }
                enumerator1HasNextValue = enumerator1.MoveNext();
                enumerator2HasNextValue = enumerator2.MoveNext();
            }

            return !enumerator1HasNextValue && !enumerator2HasNextValue;
        }
    }
}
