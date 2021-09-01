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
            Console.WriteLine("Server Open");
            while (true) ;
        }

        private static void clientConnected(object sender, PacketSocketAsyncEventArgs e) {
            e.AcceptSocket.ReceiveCompleted += new EventHandler<PacketSocketAsyncEventArgs>(PacketReceived);
            Console.WriteLine("Client Connected : {0}", e.AcceptSocket.RemoteEndPoint.ToString());
            Packet packet = new StringPacket(new string[] { "Server->Client" });
            packet.type = 255;
            e.AcceptSocket.Send(packet);
            Console.WriteLine("Packet Post");
        }

        private static void PacketReceived(object sender, PacketSocketAsyncEventArgs e) {
            for (int i = 0; i < e.ReceivePacketAmount; i++) {
                Packet packet = e.ReceiveSocket.Receive();
                Console.WriteLine("Packet Received : {0} {1}", packet, packet.type);
                if(packet is StringPacket) {
                    Console.WriteLine(" {0}", ((StringPacket)packet).data[0]);
                }
            }
        }
    }
}
