// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-11.

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Umizoo.Infrastructure.Utilities
{
    public static class SocketUtil
    {
        public static IPAddress GetLocalIPV4(params string[] startWith)
        {
            var addresses =
                Dns.GetHostEntry(Dns.GetHostName())
                    .AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            if (startWith != null && startWith.Length > 0) {
                foreach (var start in startWith) {
                    if (string.IsNullOrWhiteSpace(start))
                        continue;

                    var enumerable = addresses.Where(ip => ip.ToString().StartsWith(start));
                    if (enumerable.Any()) return enumerable.First();
                }
            }

            return addresses.First();
        }

        public static Socket CreateSocket(int sendBufferSize, int receiveBufferSize)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.NoDelay = true;
            socket.Blocking = false;
            socket.SendBufferSize = sendBufferSize;
            socket.ReceiveBufferSize = receiveBufferSize;
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            return socket;
        }

        public static void ShutdownSocket(Socket socket)
        {
            if (socket == null) return;

            try {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception) {
            }
            finally {
                CloseSocket(socket);
            }
        }
        public static void CloseSocket(Socket socket)
        {
            if (socket == null) return;

            try {
                socket.Close(10000);
            }
            catch (Exception) {
            }
        }
    }
}