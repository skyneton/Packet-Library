using SocketPacket.Network;
using SocketPacket.PacketSocket;
using System;
using System.Net;
using System.Net.Sockets;

namespace SampleServer {
    class Program {
        private static PacketSocket socket = new PacketSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // Create PacketSocket

        static void Main(string[] args)
        {
            socket.NoDelay = true; // Use NoDelay
            socket.SendTimeout = 500; // Send Max Time - 500

            socket.Bind(new IPEndPoint(IPAddress.Any, 2555)); // Port Bind
            socket.AcceptCompleted += new EventHandler<PacketSocketAsyncEventArgs>(ClientConnected); // Add Client Accept Event
            socket.DisconnectCompleted += new EventHandler<PacketSocketAsyncEventArgs>(ClientAndServerDisconnected);
            Console.WriteLine("Server Open");
            while (true) ; // Prevent Application Close
        }

        private static void ClientConnected(object sender, PacketSocketAsyncEventArgs e)
        {
            e.AcceptSocket.ReceiveCompleted += new EventHandler<PacketSocketAsyncEventArgs>(PacketReceived); // Add Client Packet Received Event
            Console.WriteLine("Client Connected : {0}", e.AcceptSocket.RemoteEndPoint.ToString());
            Packet packet = new StringPacket(new string[] { "Server->Client" }); // String Packet Create
            packet.type = 255; // Packet Type Change
            e.AcceptSocket.Send(packet); // Packet Post
            Console.WriteLine("Packet Post");
        }

        private static void ClientAndServerDisconnected(object sender, PacketSocketAsyncEventArgs e)
        {
            if (e.DisconnectSocket != socket) // if disconnected socket is not Server socket
                Console.WriteLine("Client Disconnected : {0}", e.DisconnectSocket.RemoteEndPoint.ToString());
            else
                Console.WriteLine("Server Closed");
        }

        private static void PacketReceived(object sender, PacketSocketAsyncEventArgs e)
        {
            for (int i = 0; i < e.ReceivePacketAmount; i++) { // loop Received Packet Amount
                // Packet packet = e.ReceivePacket; //Get Received Packet
                Packet packet = e.ReceiveSocket.Receive(); // Get Packet Queue And Remove
                Console.WriteLine("Packet Received : {0} {1}", packet, packet.type);
                if(packet is StringPacket) {
                    Console.WriteLine(" {0}", ((StringPacket)packet).data[0]); // Packet String Print
                }
            }
        }
    }
}
