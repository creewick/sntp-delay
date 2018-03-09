using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;

namespace Server
{
    public class SntpServer
    {
        private readonly Socket Socket;
        private readonly byte[] Buffer;
        private readonly Queue<byte[]> Queue;
        
        public SntpServer(int[] settings) : this(settings[0], settings[1], settings[2]){}

        public SntpServer(int port, int bufferSize, int threadsCount)
        {
            Socket = GetUdpSocket(port);
            Buffer = new byte[bufferSize];
            Queue = new Queue<byte[]>();
        }

        public void Start()
        {
            while (true)
            {
                try
                {
                    var size = Socket.Receive(Buffer);
                    Queue.Enqueue(Buffer.Take(size).ToArray());
                }
                catch (SocketException e)
                {
                    Console.WriteLine("{0} Error code: {1}.", e.Message, e.ErrorCode);
                    return;
                }

            }
        }
        
        private static Socket GetUdpSocket(int port)
        {
            var socket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Dgram,
                ProtocolType.Udp);
            socket.Bind(new IPEndPoint(
                IPAddress.Loopback, 
                port));
            return socket;
        }
    }
}