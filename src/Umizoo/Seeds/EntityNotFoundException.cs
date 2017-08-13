// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

using System;
using System.Runtime.Serialization;

namespace Umizoo.Seeds
{
    /// <summary>
    ///     表示一个尝试获取一个不存在的实体的异常
    /// </summary>
    [Serializable]
    public class EntityNotFoundException : ApplicationException
    {
        /// <summary>
        ///     Parameterized constructor.
        /// </summary>
        public EntityNotFoundException(object entityId, Type entityType)
        {
            EntityId = entityId;
            EntityType = entityType.FullName;
        }

        protected EntityNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            EntityId = info.GetString("entityId");
            EntityType = info.GetString("entityType");
        }


        /// <summary>
        ///     获取当前异常信息。
        /// </summary>
        public override string Message
        {
            get { return string.Format("Cannot find the entity '{0}' of id '{1}'.", EntityType, EntityId); }
        } 

        /// <summary>
        ///     获取实体Id
        /// </summary>
        public object EntityId { get; }

        /// <summary>
        ///     获取实体类型名称
        /// </summary>
        public string EntityType { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("entityId", EntityId);
            info.AddValue("entityType", EntityType);
        }
    }
}