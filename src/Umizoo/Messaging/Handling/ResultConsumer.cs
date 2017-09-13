// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-09.

using System;
using System.Collections.Generic;
using System.Linq;
using Umizoo.Infrastructure.Composition;
using Umizoo.Infrastructure.Logging;
using Umizoo.Configurations;
using Umizoo.Infrastructure;

namespace Umizoo.Messaging.Handling
{
    public class ResultConsumer : Consumer<IResult>, IInitializer
    {
        private readonly Dictionary<Type, object> _resultHandlers;

        public ResultConsumer(IMessageReceiver<Envelope<IResult>> resultReceiver)
            : base(resultReceiver, ProcessingFlags.Result)
        {
            _resultHandlers = new Dictionary<Type, object>();
        }

        public void Initialize(IObjectContainer container, IEnumerable<Type> types)
        {
            types.Where(type => type.IsClass && !type.IsAbstract && typeof(IResult).IsAssignableFrom(type))
                .ForEach(resultType => {
                    var handler = container.Resolve(typeof(IEnvelopedMessageHandler<>).MakeGenericType(resultType));
                    if (handler == null) {
                        var errorMessage =
                            string.Format("not found the handler of this type('{0}') with IEnvelopedMessageHandler<>.",
                                resultType.FullName);
                        LogManager.Default.Fatal(errorMessage);
                        throw new SystemException(errorMessage);
                    }

                    _resultHandlers[resultType] = handler;
                });
        }

        protected override void Dispose(bool disposing)
        {
        }

        protected override void OnMessageReceived(Envelope<IResult> envelope)
        {
            var resultType = envelope.Body.GetType();

            ((dynamic)_resultHandlers[resultType]).Handle((dynamic)Activator.CreateInstance(typeof(Envelope<>).MakeGenericType(resultType), envelope.Body, envelope.MessageId));
        }
    }
}
