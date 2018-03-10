using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public class SntpServer
    {
        private readonly Socket socket;
        private readonly Socket timeSocket;
        private readonly byte[] buffer;
        private readonly int port;
        private readonly double lie;


        public SntpServer(int[] settings) : this(settings[0], settings[1]){}

        public SntpServer(int port, double lie)
        {
            this.port = port;
            this.lie = lie;
            socket = GetUdpSocket(port);
            timeSocket = GetTimeSocket();
            buffer = new byte[48];
        }

        public void Start()
        {
            Console.CancelKeyPress += (sender, args) => Close();
            try
            {
                while (true)
                {
                    var remoteEP = (EndPoint) new IPEndPoint(IPAddress.Any, port);
                    socket.ReceiveFrom(buffer, ref remoteEP);
                    Console.WriteLine("Received from {0}", ((IPEndPoint) remoteEP).Address);
                    var message = new NtpMessage(buffer);
                    
                    var timeMessage = new byte[48];
                    timeMessage[0] = 0b0001_1011;
                    timeSocket.Send(timeMessage);
                    timeSocket.Receive(timeMessage);
                    socket.SendTo(timeMessage, remoteEP);
                    
                    //socket.SendTo(message.GetAnswer(lie).ToBytes(), remoteEP);
                    Console.WriteLine("Answer sent");
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("{0} Error code: {1}.", e.Message, e.ErrorCode);
            }
            finally
            {
                Close();
            }
        }

        public void Close()
        {
            socket.Close();
            timeSocket.Close();
            Console.WriteLine("Disposed the port.");
        }

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