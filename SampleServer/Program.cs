using System;
using PacketSocket.Network;
using PacketSocket.Network.Event;
using PacketSocket.Network.Sockets;

namespace SampleServer
{
    class Program
    {
        const int Port = 5000;
        
        private static PacketListener _listener;
        static void Main(string[] args)
        {
            _listener = new PacketListener(Port);
            _listener.Server.NoDelay = true;
            _listener.Server.SendTimeout = 500;
            
            _listener.RegisterAcceptEvent(new EventHandler<PacketSocketEventArgs>(ClientAccepted));
            _listener.RegisterDisconnectEvent(new EventHandler<PacketSocketEventArgs>(ClientDisconnected));
            
            PacketManager.RegisterPacket(new StringPacket());
            
            _listener.Start();

            while (true) ;
        }

        private static void ClientAccepted(object sender, PacketSocketEventArgs e)
        {
            Console.WriteLine("Client Accepted : {0}", e.AcceptClient.Client.RemoteEndPoint);
            e.AcceptClient.Timeout = 60 * 1000;
            e.AcceptClient.Client.NoDelay = true;
            e.AcceptClient.RegisterReceiveEvent(PacketReceived);
        }

        private static void PacketReceived(object sender, PacketSocketEventArgs e)
        {
            Console.WriteLine("Packet Received : {0}", e.ReceivePacket);
            
            if (e.ReceivePacket is StringPacket packet)
            {
                Console.WriteLine("\tData: {0}", packet.Data);
            }
            
            e.ReceiveClient.SendPacket(new StringPacket()
            {
                Data = "Server->Client"
            });
        }

        private static void ClientDisconnected(object sender, PacketSocketEventArgs e)
        {
            Console.WriteLine("Client Disconnected : {0}", e.DisconnectClient);
        }
    }
}