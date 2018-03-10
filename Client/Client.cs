using System;
using System.Net.Sockets;

namespace Client
{
    public class Client
    {
        private readonly Socket socket;
        
        public Client()
        {
            socket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Dgram,
                ProtocolType.Udp);
            socket.Connect("127.0.0.1", 123);
        }

        public void Start()
        {
            var message = new byte[48];
            message[0] = 0x1b;
            socket.Send(message);
            var size = socket.Receive(message);
            for (var i = 0; i < size; i++)
            {
                Console.Write(message[i]);
                Console.Write(",");
            }
        }
    }
}