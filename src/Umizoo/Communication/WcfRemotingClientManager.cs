

namespace Umizoo.Communication
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Linq;
    using System.ServiceModel;
    using System.Text.RegularExpressions;
    using System.Threading;
    using Umizoo.Infrastructure;

    public class WcfRemotingClientManager : DisposableObject
    {
        public readonly static WcfRemotingClientManager Instance = new WcfRemotingClientManager();


        private const string ipRegular =
            @"^((\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.){3}(\d{1,2}|1\d\d|2[0-4]\d|25[0-5]):(\d{2,5})$";

        private readonly ConcurrentDictionary<string, ChannelFactory<IContract>> channelFactoryDict;
        private readonly Timer timer;

        protected WcfRemotingClientManager()
        {
            this.channelFactoryDict = new ConcurrentDictionary<string, ChannelFactory<IContract>>();
            this.timer = new Timer(ScanInactiveChannelFactories, null, 5000, 5000);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                channelFactoryDict.Values.ForEach(CloseChannelFactory);
                using (timer) { }
            }
        }

        private void ScanInactiveChannelFactories(object state)
        {
            var inactiveList = channelFactoryDict.Where(item => item.Value.State != CommunicationState.Opened).ToArray();

            foreach (var pair in inactiveList) {
                if (channelFactoryDict.TryRemove(pair.Key)) {
                    if (LogManager.Default.IsInfoEnabled) {
                        LogManager.Default.InfoFormat("Removed disconnected channel factory, remotingAddress: {0}", pair.Key);
                    }
                }
            }
        }

        private void CloseChannelFactory(ChannelFactory channelFactory)
        {
            using (channelFactory) {
                channelFactory.Close();
            }
        }

        public void CloseChannelFactory(string address)
        {
            ChannelFactory<IContract> client;

            if (channelFactoryDict.TryRemove(address, out client)) {
                CloseChannelFactory(client);
            }
        }

        public ChannelFactory<IContract> GetChannelFactory(string configurationNameOrIpAddress)
        {
            return channelFactoryDict.GetOrAdd(configurationNameOrIpAddress, CreateChannelFactory);
        }

        private ChannelFactory<IContract> CreateChannelFactory(string configurationNameOrIpAddress)
        {
            var stopwatch = Stopwatch.StartNew();
            var maxtime = TimeSpan.FromMinutes(5);
            var retryTimes = 0;
            while (true) {
                retryTimes++;

                try {
                    if(Regex.IsMatch(configurationNameOrIpAddress, ipRegular)) {
                        var remoteAddress = string.Format("net.tcp://{0}/reply", configurationNameOrIpAddress);
                        return new ChannelFactory<IContract>(new NetTcpBinding(), remoteAddress);
                    }

                    return new ChannelFactory<IContract>(configurationNameOrIpAddress);
                }
                catch (Exception ex) {
                    if (stopwatch.Elapsed > maxtime) {
                        stopwatch.Stop();
                        LogManager.Default.Error(ex, "Unable to connect to remote host.");
                        break;
                    }
                }
                var waitTime = Math.Min((int)Math.Pow(2, retryTimes), 30);
                Thread.Sleep(waitTime * 1000);
            }

            return null;
        }
    }
}
