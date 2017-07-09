namespace Umizoo.Seeds
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// 表示一个尝试获取一个不存在的实体的异常
    /// </summary>
    [Serializable]
    public class EntityNotFoundException : ApplicationException
    {
        private readonly object entityId;
        private readonly string entityType;

        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public EntityNotFoundException(object entityId, Type entityType)
        {
            this.entityId = entityId;
            this.entityType = entityType.FullName;
        }

        protected EntityNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.entityId = info.GetString("entityId");
            this.entityType = info.GetString("entityType");
        }



        /// <summary>
        /// 获取当前异常信息。
        /// </summary>
        public override string Message
        {
            get
            {
                return string.Format("Cannot find the entity '{0}' of id '{1}'.", entityType, entityId);
            }
        }
        /// <summary>
        /// 获取实体Id
        /// </summary>
        public object EntityId
        {
            get { return this.entityId; }
        }
        /// <summary>
        /// 获取实体类型名称
        /// </summary>
        public string EntityType
        {
            get { return this.entityType; }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("entityId", this.entityId);
            info.AddValue("entityType", this.entityType);
        }
    }
}
