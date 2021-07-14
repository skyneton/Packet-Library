using SocketPacket.Network;
using SocketPacket.PacketSocket;
using System;
using System.Net;
using System.Net.Sockets;

namespace SampleServer {
    class Program {
        private static PacketSocket socket = new PacketSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        static void Main(string[] args) {
            socket.NoDelay = true;
            socket.SendTimeout = 500;

            socket.Bind(new IPEndPoint(IPAddress.Any, 2555));
            socket.AcceptCompleted += new EventHandler<PacketSocketAsyncEventArgs>(clientConnected);
            Console.WriteLine("서버 오픈");
            while (true) ;
        }

        private static void clientConnected(object sender, PacketSocketAsyncEventArgs e) {
            Console.WriteLine("클라이언트 연결됨 : {0}", e.AcceptSocket.RemoteEndPoint.ToString());
            Packet packet = new Packet();
            packet.type = 255;
            e.AcceptSocket.Send(packet);
            Console.WriteLine("패킷 보냄");
        }
    }
}
