// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-11.

using System;
using System.ServiceModel;
using Umizoo.Infrastructure;
using Umizoo.Infrastructure.Async;
using Umizoo.Infrastructure.Logging;

namespace Umizoo.Communication
{
    public abstract class WcfServerChannel : Processor, IChannel
    {
        private readonly string _remoteAddress;
        private readonly ProtocolCode _protocolCode;
        private ServiceHost _wcfHost;

        protected WcfServerChannel(ProtocolCode protocol)
        {
            _protocolCode = protocol;
        }

        protected WcfServerChannel(string host, int port, ProtocolCode protocol)
            : this(protocol)
        {
            Assertions.NotNullOrEmpty(host, "host");
            if (port <= 1024 || port >= 65535)
            {
                throw new ArgumentException("port is only 1025 to 65534", "port");
            }

            _remoteAddress = string.Format("net.tcp://{0}:{1}/{2}", host, port, protocol.ToString().ToLower());
        }

        protected IAsyncResult CreateInnerAsyncResult(AsyncCallback asyncCallback, object asyncState)
        {
            SimpleAsyncResult asyncResult = new SimpleAsyncResult(asyncState);
            asyncResult.MarkCompleted(true /* completedSynchronously */, asyncCallback);
            return asyncResult;
        }

        public abstract IAsyncResult BeginExecute(Request request, AsyncCallback callback, object state);

        public virtual Response EndExecute(IAsyncResult asyncResult)
        {
            return WrappedAsyncResult<Response>.Cast(asyncResult, null).End();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Stop();
        }

        protected override void Start()
        {
            _wcfHost = new ServiceHost(this);
            if (string.IsNullOrEmpty(_remoteAddress))
            {
                _wcfHost.AddServiceEndpoint(typeof(IChannel), new NetTcpBinding(), _remoteAddress);
            }
            _wcfHost.Opened += (sender, e) => LogManager.Default.InfoFormat("WCF {0} Service Started.", _protocolCode);
            _wcfHost.Open();
        }

        protected override void Stop()
        {
            using (_wcfHost) {
                _wcfHost.Closed += (sender, e) => LogManager.Default.InfoFormat("WCF {0} Service Stopped.", _protocolCode);
                _wcfHost.Close();
                _wcfHost = null;
            }
        }
    }

}