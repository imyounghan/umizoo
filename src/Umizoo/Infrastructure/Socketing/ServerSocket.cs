

namespace Umizoo.Infrastructure.Socketing
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using Umizoo.Infrastructure.Socketing.BufferManagement;
    using Umizoo.Infrastructure.Utilities;

    public class ServerSocket
    {
        #region Private Variables

        private readonly Socket _socket;
        private readonly SocketSetting _setting;
        private readonly IPEndPoint _listeningEndPoint;
        private readonly SocketAsyncEventArgs _acceptSocketArgs;
        private readonly IList<IConnectionEventListener> _connectionEventListeners;
        private readonly Action<ITcpConnection, byte[], Action<byte[]>> _messageArrivedHandler;
        private readonly IBufferPool _receiveDataBufferPool;

        #endregion

        public ServerSocket(IPEndPoint listeningEndPoint, SocketSetting setting, IBufferPool receiveDataBufferPool, Action<ITcpConnection, byte[], Action<byte[]>> messageArrivedHandler)
        {
            Ensure.NotNull(listeningEndPoint, "listeningEndPoint");
            Ensure.NotNull(setting, "setting");
            Ensure.NotNull(receiveDataBufferPool, "receiveDataBufferPool");
            Ensure.NotNull(messageArrivedHandler, "messageArrivedHandler");

            _listeningEndPoint = listeningEndPoint;
            _setting = setting;
            _receiveDataBufferPool = receiveDataBufferPool;
            _connectionEventListeners = new List<IConnectionEventListener>();
            _messageArrivedHandler = messageArrivedHandler;
            _socket = SocketUtil.CreateSocket(_setting.SendBufferSize, _setting.ReceiveBufferSize);
            _acceptSocketArgs = new SocketAsyncEventArgs();
            _acceptSocketArgs.Completed += AcceptCompleted;
        }

        public void RegisterConnectionEventListener(IConnectionEventListener listener)
        {
            _connectionEventListeners.Add(listener);
        }
        public void Start()
        {
            if (LogManager.Default.IsInfoEnabled) {
                LogManager.Default.InfoFormat("Socket server is starting, listening on TCP endpoint: {0}.", _listeningEndPoint);
            }

            try {
                _socket.Bind(_listeningEndPoint);
                _socket.Listen(5000);
            }
            catch (Exception ex) {
                LogManager.Default.Error(ex, "Failed to listen on TCP endpoint: {0}.", _listeningEndPoint);
                SocketUtil.ShutdownSocket(_socket);
                throw;
            }

            StartAccepting();
        }
        public void Shutdown()
        {
            SocketUtil.ShutdownSocket(_socket);

            if (LogManager.Default.IsInfoEnabled) {
                LogManager.Default.InfoFormat("Socket server shutdown, listening TCP endpoint: {0}.", _listeningEndPoint);
            }
        }

        private void StartAccepting()
        {
            try {
                var firedAsync = _socket.AcceptAsync(_acceptSocketArgs);
                if (!firedAsync) {
                    ProcessAccept(_acceptSocketArgs);
                }
            }
            catch (ObjectDisposedException) {
            }
            catch (Exception ex) {
                LogManager.Default.Info("Socket accept has exception, try to start accepting one second later.", ex);
                Thread.Sleep(1000);
                StartAccepting();
            }
        }
        private void AcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }
        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            try {
                if (e.SocketError == SocketError.Success) {
                    var acceptSocket = e.AcceptSocket;
                    e.AcceptSocket = null;
                    OnSocketAccepted(acceptSocket);
                }
                else {
                    SocketUtil.ShutdownSocket(e.AcceptSocket);
                    e.AcceptSocket = null;
                }
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex) {
                LogManager.Default.Error("Process socket accept has exception.", ex);
            }
            finally {
                StartAccepting();
            }
        }

        private void OnSocketAccepted(Socket socket)
        {
            Task.Factory.StartNew(() => {
                try {
                    var connection = new TcpConnection(socket, _setting, _receiveDataBufferPool, OnMessageArrived, OnConnectionClosed);

                    if (LogManager.Default.IsInfoEnabled) {
                        LogManager.Default.InfoFormat("Socket accepted, remote endpoint:{0}", socket.RemoteEndPoint);
                    }

                    foreach (var listener in _connectionEventListeners) {
                        try {
                            listener.OnConnectionAccepted(connection);
                        }
                        catch (Exception ex) {
                            LogManager.Default.Error(ex, "Notify connection accepted failed, listener type:{0}.", listener.GetType().Name);
                        }
                    }
                }
                catch (ObjectDisposedException) { }
                catch (Exception ex) {
                    LogManager.Default.Info("Accept socket client has unknown exception.", ex);
                }
            });
        }
        private void OnMessageArrived(ITcpConnection connection, byte[] message)
        {
            try {
                _messageArrivedHandler(connection, message, reply => connection.QueueMessage(reply));
            }
            catch (Exception ex) {
                LogManager.Default.Error("Handle message error.", ex);
            }
        }
        private void OnConnectionClosed(ITcpConnection connection, SocketError socketError)
        {
            foreach (var listener in _connectionEventListeners) {
                try {
                    listener.OnConnectionClosed(connection, socketError);
                }
                catch (Exception ex) {
                    LogManager.Default.Error(ex, "Notify connection closed failed, listener type:{0}", listener.GetType().Name);
                }
            }
        }
    }
}