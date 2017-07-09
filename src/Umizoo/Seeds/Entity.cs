

namespace Umizoo.Seeds
{
    using System;
    using System.Runtime.Serialization;

    using Umizoo.Infrastructure;

    /// <summary>
    /// 表示继承该抽象类的是一个实体
    /// </summary>
    [DataContract]
    [Serializable]
    public abstract class Entity<TIdentify> : IUniquelyIdentifiable
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        protected Entity()
        { }

        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        protected Entity(TIdentify id)
        {
            this.Id = id;
        }

        /// <summary>
        /// 标识ID
        /// </summary>
        [DataMember(Name = "id")]
        public virtual TIdentify Id { get; protected set; }


        /// <summary>
        /// 确定此实例是否与指定的对象相同。
        /// </summary>
        public override bool Equals(object obj)
        {
            if(obj == null || obj.GetType() != this.GetType()) {
                return false;
            }

            if(Object.ReferenceEquals(this, obj)) {
                return true;
            }

            var other = obj as Entity<TIdentify>;
            if(other == null)
                return false;

            return other.Id.Equals(this.Id);
        }

        /// <summary>
        /// 返回此实例的哈希代码
        /// </summary>
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        /// <summary>
        /// 输出字符串格式
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0}@{1}", this.GetType().FullName, this.Id);
        }

        /// <summary>
        /// 判断两个实例是否相同
        /// </summary>
        public static bool operator ==(Entity<TIdentify> left, Entity<TIdentify> right)
        {
            return IsEqual(left, right);
        }

        /// <summary>
        /// 判断两个实例是否不相同
        /// </summary>
        public static bool operator !=(Entity<TIdentify> left, Entity<TIdentify> right)
        {
            return !(left == right);
        }


        private static bool IsEqual(Entity<TIdentify> left, Entity<TIdentify> right)
        {
            if(ReferenceEquals(left, null) ^ ReferenceEquals(right, null)) {
                return false;
            }
            return ReferenceEquals(left, null) || left.Equals(right);
        }

        #region IUniquelyIdentifiable 成员

        string IUniquelyIdentifiable.Id
        {
            get
            {
                if(Id != null) {
                    return Id.ToString();
                }
                return null;
            }
        }

        #endregion
    }
}
