using SocketPacket.Network;
using SocketPacket.PacketSocket;
using System;
using System.Net;
using System.Net.Sockets;

namespace SampleClient {
    class Program {
        private static PacketSocket socket = new PacketSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static void Main(string[] args) {
            socket.NoDelay = true;
            socket.SendTimeout = 100;
            socket.ConnectCompleted += new EventHandler<PacketSocketAsyncEventArgs>(SocketConnected);
            socket.ReceiveCompleted += new EventHandler<PacketSocketAsyncEventArgs>(PacketReceived);
            socket.DisconnectCompleted += new EventHandler<PacketSocketAsyncEventArgs>(Disconneced);

            socket.ConnectTimeout(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2555), 1000);
            //socket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2555));
            while (true) {
                Console.ReadLine();
                if (socket.Connected) {
                    Packet packet = new Packet();
                    packet.type = 15;
                    socket.Send(packet);
                    Console.WriteLine("패킷 보냄");
                }
            }
        }

        private static void SocketConnected(object sender, PacketSocketAsyncEventArgs e) {
            Console.WriteLine("서버 연결 성공");
        }

        private static void PacketReceived(object sender, PacketSocketAsyncEventArgs e) {
            Console.WriteLine("패킷 받음 : {0} {1}", e.ReceivePacket, e.ReceivePacket.type);
        }

        private static void Disconneced(object sender, PacketSocketAsyncEventArgs e) {
            Console.WriteLine("접속 종료");
        }
    }
}
