using System;

namespace Umizoo.Infrastructure.Composition
{
    /// <summary>
    /// 类型注册
    /// </summary>
    public sealed class TypeRegistration
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeRegistration"/> class. 
        /// </summary>
        public TypeRegistration(Type type)
            : this(type, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeRegistration"/> class. 
        /// </summary>
        public TypeRegistration(Type type, string name)
        {
            this.Type = type;
            this.Name = name;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     名称
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     类型
        /// </summary>
        public Type Type { get; private set; }

        #endregion

        #region Methods and Operators

        /// <summary>
        /// 判断该实例与当前实例是否相同
        /// </summary>
        public override bool Equals(object obj)
        {
            var other = obj as TypeRegistration;

            if(other == null) {
                return false;
            }

            if(ReferenceEquals(this, other)) {
                return true;
            }

            if(this.Type != other.Type) {
                return false;
            }

            if(!string.Equals(this.Name, other.Name, StringComparison.Ordinal)) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取该实例的哈希代码
        /// </summary>
        public override int GetHashCode()
        {
            if(string.IsNullOrEmpty(this.Name)) {
                return this.Type.GetHashCode();
            }

            return this.Type.GetHashCode() ^ this.Name.GetHashCode();
        }

        #endregion
    }
}
