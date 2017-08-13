// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-10.

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading;
using Umizoo.Infrastructure;
using Umizoo.Infrastructure.Logging;

namespace Umizoo.Communication
{
    public class WcfClientChannelFactory : DisposableObject, IClientChannelFactory
    {
        private const string IpRegular =
            @"^((\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.){3}(\d{1,2}|1\d\d|2[0-4]\d|25[0-5]):(\d{2,5})$";

        private readonly ConcurrentDictionary<string, ChannelFactory<IChannel>> channelFactoryDict;

        public WcfClientChannelFactory()
        {
            channelFactoryDict = new ConcurrentDictionary<string, ChannelFactory<IChannel>>();
        }

        public IChannel GetChannel(string configurationNameOrIpAddress, ProtocolCode protocol)
        {
            if (Regex.IsMatch(configurationNameOrIpAddress, IpRegular))
            {
                var remoteUrl = string.Format("net.tcp://{0}/{1}", configurationNameOrIpAddress,
                    protocol.ToString().ToLower());
                return channelFactoryDict.AddOrUpdate(remoteUrl, CreateChannelFactoryByAddress, (url, original) =>
                {
                    if (original == null || original.State != CommunicationState.Opened)
                    {
                        CloseChannelFactory(original);
                        if (LogManager.Default.IsInfoEnabled)
                            LogManager.Default.InfoFormat(
                                "reconnect the disconnected channel factory, remotingAddress: {0}.", url);
                        return CreateChannelFactoryByAddress(url);
                    }
                    return original;
                }).CreateChannel();
            }

            return channelFactoryDict.AddOrUpdate(configurationNameOrIpAddress, CreateChannelFactoryByConfig,
                (name, original) =>
                {
                    if (original == null || original.State != CommunicationState.Opened)
                    {
                        CloseChannelFactory(original);
                        if (LogManager.Default.IsInfoEnabled)
                            LogManager.Default.InfoFormat(
                                "reconnect the disconnected channel factory, configurationName: {0}.", name);
                        return CreateChannelFactoryByConfig(name);
                    }
                    return original;
                }).CreateChannel();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                channelFactoryDict.Values.ForEach(CloseChannelFactory);
                channelFactoryDict.Clear();
            }
        }

        private void CloseChannelFactory(ChannelFactory channelFactory)
        {
            if (channelFactory.IsNull())
                return;

            using (channelFactory)
            {
                channelFactory.Close();
            }
        }

        private ChannelFactory<IChannel> CreateChannelFactoryByConfig(string configurationName)
        {
            return CreateChannelFactory(() => new ChannelFactory<IChannel>(configurationName));
        }

        private ChannelFactory<IChannel> CreateChannelFactoryByAddress(string ipAddress)
        {
            return CreateChannelFactory(() => new ChannelFactory<IChannel>(new NetTcpBinding(), ipAddress));
        }

        private ChannelFactory<IChannel> CreateChannelFactory(Func<ChannelFactory<IChannel>> builder)
        {
            var stopwatch = Stopwatch.StartNew();
            var maxtime = TimeSpan.FromMinutes(5);
            var retryTimes = 0;
            while (true)
            {
                retryTimes++;

                try
                {
                    return builder.Invoke();
                }
                catch (Exception ex)
                {
                    if (stopwatch.Elapsed > maxtime)
                    {
                        stopwatch.Stop();
                        LogManager.Default.Error(ex, "Unable to connect to remote host.");
                        break;
                    }
                }
                var waitTime = Math.Min((int) Math.Pow(2, retryTimes), 30);
                Thread.Sleep(waitTime * 1000);
            }

            return null;
        }
    }
}