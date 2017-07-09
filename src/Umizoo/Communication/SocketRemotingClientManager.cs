using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;

using Umizoo.Infrastructure;
using Umizoo.Infrastructure.Remoting;

namespace Umizoo.Communication
{
    public class SocketRemotingClientManager : DisposableObject
    {
        public readonly static SocketRemotingClientManager Instance = new SocketRemotingClientManager();

        private readonly ConcurrentDictionary<string, SocketRemotingClient> remotingClientDict;
        private readonly Timer timer;

        protected SocketRemotingClientManager()
        {
            this.remotingClientDict = new ConcurrentDictionary<string, SocketRemotingClient>();
            this.timer = new Timer(ScanInactiveRemotingClients, null, 5000, 5000);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                this.timer.Dispose();
                this.remotingClientDict.Values.ForEach(CloseRemotingClient);
                this.remotingClientDict.Clear();
            }
        }

        private void CloseRemotingClient(SocketRemotingClient client)
        {
            client.Shutdown();
        }

        private void ScanInactiveRemotingClients(object state)
        {
            var inactiveList = remotingClientDict.Where(item => !item.Value.IsConnected).ToArray();

            foreach (var pair in inactiveList) {
                if (remotingClientDict.TryRemove(pair.Key)) {
                    if (LogManager.Default.IsInfoEnabled) {
                        LogManager.Default.InfoFormat("Removed disconnected remoting client, remotingAddress: {0}", pair.Key);
                    }
                }
            }
        }

        public void CloseRemotingClient(string address)
        {
            SocketRemotingClient client;

            if(remotingClientDict.TryRemove(address, out client)) {
                CloseRemotingClient(client);
            }
        }

        public SocketRemotingClient GetRemotingClient(string address)
        {
            return remotingClientDict.GetOrAdd(address, CreateRemotingClient);
        }

        private SocketRemotingClient CreateRemotingClient(string address)
        {
            ThrowIfDisposed();

            var retryTimes = 0;
            var stopwatch = Stopwatch.StartNew();
            var maxtime = TimeSpan.FromMinutes(5);
            while (true) {
                try {
                    var items = address.Split(':');
                    var ipEndPoint = new IPEndPoint(IPAddress.Parse(items[0]), items[1].Change<int>());

                    return new SocketRemotingClient(ipEndPoint).Start();
                }
                catch (Exception ex) {
                    if (stopwatch.Elapsed > maxtime) {
                        stopwatch.Stop();
                        LogManager.Default.Error(ex, "Unable to connect to remote host:({0}), retry times:{1}.", retryTimes);
                        break;
                    }
                }
                var waitTime = Math.Min((int)Math.Pow(2, ++retryTimes), 60);
                Thread.Sleep(waitTime * 500);
            }

            return null;
        }
    }
}
