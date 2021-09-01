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
                    Packet packet = new StringPacket(new string[]{ "Client->Server" });
                    packet.type = 15;
                    socket.Send(packet);
                    Console.WriteLine("Packet Post");
                }
            }
        }

        private static void SocketConnected(object sender, PacketSocketAsyncEventArgs e) {
            Console.WriteLine("Server Connected");
        }

        private static void PacketReceived(object sender, PacketSocketAsyncEventArgs e) {
            for (int i = 0; i < e.ReceivePacketAmount; i++) {
                Packet packet = e.ReceiveSocket.Receive();
                Console.WriteLine("Packet Received : {0} {1}", packet, packet.type);
                if (packet is StringPacket) {
                    Console.WriteLine(" {0}", ((StringPacket)packet).data[0]);
                }
            }
        }

        private static void Disconneced(object sender, PacketSocketAsyncEventArgs e) {
            Console.WriteLine("Disconnect");
        }
    }
}
