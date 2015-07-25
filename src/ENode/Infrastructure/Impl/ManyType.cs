using System;
using System.Collections.Generic;
using System.Linq;

namespace ENode.Infrastructure.Impl
{
    public class ManyType
    {
        private IList<Type> _types = new List<Type>();

        public ManyType(params Type[] types)
        {
            if (types.Any(x => types.Any(y => y == x && !ReferenceEquals(x, y))))
            {
                throw new NotSupportedException("Invalid ManyType:" + string.Join("|", types.Select(x => x.Name)));
            }
            _types = types;
        }

        public IEnumerable<Type> GetTypes()
        {
            return _types;
        }

        public static bool operator ==(ManyType left, ManyType right)
        {
            return IsEqual(left, right);
        }
        public static bool operator !=(ManyType left, ManyType right)
        {
            return !IsEqual(left, right);
        }

        public override int GetHashCode()
        {
            return _types.Select(x => x.GetHashCode()).Aggregate((x, y) => x ^ y);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj == null)
            {
                return false;
            }

            var other = obj as ManyType;
            if (other == null)
            {
                return false;
            }
            if (this._types.Count != other._types.Count)
            {
                return false;
            }

            foreach (var type in _types)
            {
                if (!other._types.Any(x => x == type))
                {
                    return false;
                }
            }

            return true;
        }
        private static bool IsEqual(ManyType left, ManyType right)
        {
            if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
            {
                return false;
            }
            return ReferenceEquals(left, null) || left.Equals(right);
        }
    }
}
