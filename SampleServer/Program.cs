using System;
using System.Linq;
using PacketSocket.Network;
using PacketSocket.Network.Event;
using PacketSocket.Network.Sockets;

namespace SampleServer
{
    internal static class Program
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
            PacketManager.RegisterPacket(new RoomPacket());
            PacketManager.RegisterPacket(new LeavePacket());
            
            _listener.Start();

            while (true)
            {
                var command = Console.ReadLine();
                if(command == null) continue;
                if (command.StartsWith("!"))
                {
                    Console.WriteLine(_listener.Clients.Count);
                }else if (command.StartsWith("."))
                {
                    Console.WriteLine(Room.Rooms.Count);
                }
            }
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

            switch (e.ReceivePacket)
            {
                case StringPacket packet:
                    Console.WriteLine("\tData: {0}", packet.Data);
                
                    if(e.ReceiveClient.Rooms.Count == 0)
                        e.ReceiveClient.SendPacket(new StringPacket()
                        {
                            Data = packet.Data
                        });
                    else
                        foreach (var room in e.ReceiveClient.Rooms)
                        {
                            room.SendPacket(new StringPacket()
                            {
                                Data = packet.Data
                            });
                        }

                    break;
                case RoomPacket packet:
                    e.ReceiveClient.Join(packet.RoomName);
                    break;
                case LeavePacket packet:
                    e.ReceiveClient.Leave(packet.RoomName);
                    break;
            }
        }

        private static void ClientDisconnected(object sender, PacketSocketEventArgs e)
        {
            Console.WriteLine("Client Disconnected : {0}", e.DisconnectClient);
        }
    }
}