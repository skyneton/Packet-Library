using System;
using PacketSocket.Network;
using PacketSocket.Network.Event;
using PacketSocket.Network.Sockets;

namespace SampleClient
{
    class Program
    {
        const int Port = 5000;
        
        private static PacketClient _client;
        static void Main(string[] args)
        {
            _client = new PacketClient();
            _client.NoDelay = true;
            _client.Timeout = 60 * 1000;
            
            _client.RegisterConnectEvent(new EventHandler<PacketSocketEventArgs>(ServerConnected));
            _client.RegisterReceiveEvent(new EventHandler<PacketSocketEventArgs>(PacketReceived));
            _client.RegisterDisconnectEvent(new EventHandler<PacketSocketEventArgs>(ServerDisconnected));
            
            PacketManager.RegisterPacket(new StringPacket());
            
            // _client.Connect("127.0.0.1", Port);
            _client.ConnectTimeout("127.0.0.1", Port, 1000);
            Console.WriteLine("async");
            while (true)
            {
                Console.ReadLine();
                if (_client.Connected)
                {
                    _client.SendPacket(new StringPacket()
                    {
                        Data = "Client -> Server"
                    });
                }
            }
        }

        private static void ServerConnected(object sender, PacketSocketEventArgs e)
        {
            Console.WriteLine("Server Connected : {0}", e.ConnectClient.Client.RemoteEndPoint);
        }

        private static void PacketReceived(object sender, PacketSocketEventArgs e)
        {
            Console.WriteLine("Packet Received : {0}", e.ReceivePacket);
            
            if (e.ReceivePacket is StringPacket packet)
            {
                Console.WriteLine("\tData: {0}", packet.Data);
            }
        }

        private static void ServerDisconnected(object sender, PacketSocketEventArgs e)
        {
            Console.WriteLine("Server Disconnected : {0}", e.DisconnectClient);
        }
    }
}