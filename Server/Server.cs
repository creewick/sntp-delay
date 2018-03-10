using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace Server
{
    public class SntpServer
    {
        private readonly Socket socket;
        private readonly Socket timeSocket;
        private readonly byte[] buffer;
        private readonly int port;
        private readonly int lie;


        public SntpServer(int[] settings) : this(settings[0], settings[1], settings[2]){}

        public SntpServer(int port, int bufferSize, int lie)
        {
            this.port = port;
            this.lie = lie;
            socket = GetUdpSocket(port);
            timeSocket = GetTimeSocket();
            buffer = new byte[bufferSize];
        }

        public void Start()
        {
            try
            {
                while (true)
                {
                    var remoteEP = (EndPoint) new IPEndPoint(IPAddress.Any, port);
                    var size = socket.ReceiveFrom(buffer, ref remoteEP);
                    var request = buffer.Take(size).ToArray();
                    Console.WriteLine("Received from {0}", ((IPEndPoint) remoteEP).Address);
                    
                    var message = new NtpMessage(request);
                    socket.SendTo(message.GetAnswer().ToBytes(), remoteEP);
                    Console.WriteLine("Answer sent");
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("{0} Error code: {1}.", e.Message, e.ErrorCode);
            }
            finally
            {
                socket.Close();
                timeSocket.Close();
                Console.WriteLine("Disposed the port.");
            }
        }

        private static byte[] GetAnswer(byte[] request) => 
            new NtpMessage(request).GetAnswer().ToBytes();

        private static Socket GetTimeSocket()
        {
            var socket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Dgram,
                ProtocolType.Udp);
            socket.Connect("time.windows.com", 123);
            return socket;
        }
        
        private static Socket GetUdpSocket(int port)
        {
            try
            {
                var socket = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Dgram,
                    ProtocolType.Udp);
                socket.Bind(new IPEndPoint(
                    IPAddress.Loopback, 
                    port));
                Console.WriteLine("Listening to {0}", port);
                return socket;
            }
            catch (SocketException e)
            {
                Console.WriteLine("{0} Error code: {1}.", e.Message, e.ErrorCode);
                return null;
            }
        }
    }
}