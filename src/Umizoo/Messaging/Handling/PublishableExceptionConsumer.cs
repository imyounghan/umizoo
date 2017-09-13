// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

using System;
using System.Collections.Generic;
using System.Linq;
using Umizoo.Infrastructure.Composition;
using Umizoo.Configurations;
using Umizoo.Infrastructure;

namespace Umizoo.Messaging.Handling
{
    public class PublishableExceptionConsumer : MessageConsumer<IPublishableException>, IInitializer
    {
        public PublishableExceptionConsumer(IMessageReceiver<Envelope<IPublishableException>> exceptionReceiver)
            : base(exceptionReceiver, ProcessingFlags.PublishableException)
        {
        }

        public void Initialize(IObjectContainer container, IEnumerable<Type> types)
        {
            BasicTypes.PublishableExceptionTypes.Values.ForEach(exceptionType => Initialize(container, exceptionType));
        }
    }
}