using SocketPacket.Network;
using SocketPacket.PacketSocket;
using System;
using System.Net;
using System.Net.Sockets;

namespace SampleClient {
    class Program {
        private static PacketSocket socket = new PacketSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // Create PacketSocket
        static void Main(string[] args) {
            socket.NoDelay = true; // Use NoDelay
            socket.SendTimeout = 100; //Send Max Time - 100
            socket.ConnectCompleted += new EventHandler<PacketSocketAsyncEventArgs>(SocketConnected); // Add Connect Success Event
            socket.ReceiveCompleted += new EventHandler<PacketSocketAsyncEventArgs>(PacketReceived); // Add Packet Received Event
            socket.DisconnectCompleted += new EventHandler<PacketSocketAsyncEventArgs>(Disconneced); // Add Socket Disconnect Event

            socket.ConnectTimeout(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2555), 1000); // Socket Connect Async
            //socket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2555)); //Socket Connect
            while (true) {
                Console.ReadLine(); // Key Input
                if (socket.Connected) { //Check Socket is Connect
                    Packet packet = new StringPacket(new string[]{ "Client->Server" }); // String Packet Create
                    packet.type = 15; // Packet Type Change
                    socket.Send(packet); // Packet Send
                    Console.WriteLine("Packet Post");
                }
            }
        }

        private static void SocketConnected(object sender, PacketSocketAsyncEventArgs e) {
            Console.WriteLine("Server Connected");
        }

        private static void PacketReceived(object sender, PacketSocketAsyncEventArgs e) {
            for (int i = 0; i < e.ReceivePacketAmount; i++) {
                //Packet packet = e.ReceivePacket; // Get Received Packet
                Packet packet = e.ReceiveSocket.Receive(); // Get Packet Queue And Remove
                Console.WriteLine("Packet Received : {0} {1}", packet, packet.type);
                if (packet is StringPacket) {
                    Console.WriteLine(" {0}", ((StringPacket)packet).data[0]); // Packet String Print
                }
            }
        }

        private static void Disconneced(object sender, PacketSocketAsyncEventArgs e) {
            Console.WriteLine("Disconnect");
        }
    }
}
