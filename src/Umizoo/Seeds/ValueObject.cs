
namespace Umizoo.Seeds
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using Umizoo.Infrastructure;

    /// <summary>
    /// 表示继承该抽象类的类型是一个值对象
    /// </summary>
    [DataContract]
    [Serializable]
    public abstract class ValueObject
    {
        /// <summary>
        /// 判断是否相等
        /// </summary>
        public static bool operator ==(ValueObject left, ValueObject right)
        {
            return IsEqual(left, right);
        }
        /// <summary>
        /// 判断是否不相等
        /// </summary>
        public static bool operator !=(ValueObject left, ValueObject right)
        {
            return !IsEqual(left, right);
        }

        /// <summary>
        /// 获取该值对象所有属性的值。
        /// </summary>
        protected virtual IEnumerable<object> GetAtomicValues()
        {
            return this.GetType().GetProperties().Select(p => p.GetValue(this, null)).AsEnumerable();
        }

        /// <summary>
        /// 指示当前对象是否等于同一类型的另一个对象。
        /// </summary>
        public override bool Equals(object obj)
        {
            if(obj == null || obj.GetType() != this.GetType())
                return false;

            var other = (ValueObject)obj;
            if(ReferenceEquals(null, other)) {
                return false;
            }
            if(ReferenceEquals(this, other)) {
                return true;
            }

            IEnumerable enumerable1 = this.GetAtomicValues();
            IEnumerable enumerable2 = other.GetAtomicValues();

            for(IEnumerator enumerator1 = enumerable1.GetEnumerator(), enumerator2 = enumerable2.GetEnumerator();
                enumerator1.MoveNext() && enumerator2.MoveNext(); ) {
                if(ReferenceEquals(enumerator1.Current, null) ^ ReferenceEquals(enumerator2.Current, null)) {
                    return false;
                }
                if(enumerator1.Current != null && enumerator2.Current != null) {
                    if(enumerator1.Current is IEnumerable && enumerator2.Current is IEnumerable) {
                        if(!CompareEnumerables(enumerator1.Current as IEnumerable, enumerator2.Current as IEnumerable)) {
                            return false;
                        }
                    }
                    else if(!enumerator1.Current.Equals(enumerator2.Current)) {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 获取当前类型的哈希函数
        /// </summary>
        public override int GetHashCode()
        {
            return GetAtomicValues().Select(x => x != null ? x.GetHashCode() : 0).Aggregate((x, y) => x ^ y);
        }

        private static bool IsEqual(ValueObject left, ValueObject right)
        {
            if(ReferenceEquals(left, null) ^ ReferenceEquals(right, null)) {
                return false;
            }
            return ReferenceEquals(left, null) || left.Equals(right);
        }
        private static bool CompareEnumerables(IEnumerable enumerable1, IEnumerable enumerable2)
        {
            Ensure.NotNull(enumerable1, "enumerable1");
            Ensure.NotNull(enumerable2, "enumerable2");

            for (IEnumerator enumerator1 = enumerable1.GetEnumerator(), enumerator2 = enumerable2.GetEnumerator();
                enumerator1.MoveNext() && enumerator2.MoveNext(); ) {
                if(ReferenceEquals(enumerator1.Current, null) ^ ReferenceEquals(enumerator2.Current, null)) {
                    return false;
                }
                if(enumerator1.Current != null && enumerator2.Current != null) {
                    if(enumerator1.Current is IEnumerable && enumerator2.Current is IEnumerable) {
                        if(!CompareEnumerables(enumerator1.Current as IEnumerable, enumerator2.Current as IEnumerable)) {
                            return false;
                        }
                    }
                    else if(!enumerator1.Current.Equals(enumerator2.Current)) {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
