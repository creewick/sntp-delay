using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public class SntpServer
    {
        private readonly Socket socket;
        private readonly byte[] buffer;
        private readonly int port;
        private readonly double lie;


        public SntpServer(int[] settings) : this(settings[0], settings[1]){}

        public SntpServer(int port, double lie)
        {
            this.port = port;
            this.lie = lie;
            socket = GetUdpSocket(port);
            buffer = new byte[48];
            Console.CancelKeyPress += (sender, args) => Close();
        }

        public void Start()
        {
            try
            {
                while (true)
                {
                    var remoteEP = (EndPoint) new IPEndPoint(IPAddress.Any, port);
                    socket.ReceiveFrom(buffer, ref remoteEP);
                    Console.WriteLine("Received from {0}", ((IPEndPoint) remoteEP).Address);
                    
                    var message = new NtpMessage(buffer);
                    
                    socket.SendTo(message.GetAnswer(lie).ToBytes(), remoteEP);
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
            Console.WriteLine("Disposed the port.");
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