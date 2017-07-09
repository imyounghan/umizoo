
namespace Umizoo.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.Serialization;

    using Umizoo.Configurations;
    using Umizoo.Infrastructure;
    using Umizoo.Infrastructure.Composition;

    public class PublishableExceptionReceiver : KafkaReceiver<PublishableExceptionDescriptor, IPublishableException>, IInitializer
    {
        private readonly Dictionary<Type, ConstructorInfo> constructorMap;

        public PublishableExceptionReceiver(ITextSerializer serializer, ITopicProvider topicProvider)
            : base(serializer, topicProvider)
        {
            this.constructorMap = new Dictionary<Type, ConstructorInfo>();
        }

        protected override Envelope<IPublishableException> Convert(PublishableExceptionDescriptor descriptor)
        {
            var type = Configuration.Current.PublishableExceptionTypes[descriptor.TypeName];
            var serializableInfo = new SerializationInfo(type, new FormatterConverter());

            descriptor.Items.ForEach(item => {
                var baseType = Type.GetType(item.GetMetadataTypeName());
                var value = System.Convert.ChangeType(item.Value, baseType);
                serializableInfo.AddValue(item.Key, value, baseType);
            });

            var exception = (IPublishableException)constructorMap[type].Invoke(new object[] { serializableInfo });

            var envelope = new Envelope<IPublishableException>(exception, descriptor.ExceptionId);
            return envelope;
        }

        #region IInitializer 成员

        public void Initialize(IObjectContainer container, IEnumerable<Assembly> assemblies)
        {
            var serializationInfoType = typeof(SerializationInfo);

            foreach(var type in Configuration.Current.PublishableExceptionTypes.Values)
            {
                var constructor = type.GetConstructor(BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly,
                null, new Type[] { serializationInfoType }, null);

                if(constructor == null) {
                    string errorMessage = string.Format("Type '{0}' must have a constructor with the following signature: .ctor({1} info)", type.FullName, serializationInfoType.FullName);
                    throw new SystemException(errorMessage);
                }

                constructorMap[type] = constructor;
            }
        }

        #endregion
    }
}
