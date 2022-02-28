using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using PacketSocket.Network.Event;
using PacketSocket.Utils;

namespace PacketSocket.Network.Sockets
{
    public class Room
    {
        public readonly string RoomName;
        
        private static readonly ConcurrentDictionary<string, Room> RoomGroups = new();
        private readonly ConcurrentBag<PacketClient> _clients = new();

        public static ReadOnlyCollection<Room> Rooms => new(RoomGroups.Values.ToList());
        public ReadOnlyCollection<PacketClient> Clients => new(_clients.ToList());

        private static event EventHandler<PacketSocketEventArgs> RoomCreateCompleted;
        private static event EventHandler<PacketSocketEventArgs> RoomDestroyCompleted;

        private Room(string room)
        {
            RoomName = room;
        }

        /// <summary>
        /// Send packets to all clients connected to the room.
        /// </summary>
        /// <param name="packet">Your packet.</param>
        public void SendPacket(IPacket packet)
        {
            var buf = new ByteBuf();
            buf.WriteVarInt(packet.PacketKey);
            packet.Write(buf);

            var data = buf.Flush();
            
            foreach (var client in _clients)
            {
                client.SendByteArray(data);
            }
        }
        
        internal static Room GetOrAdd(string roomName)
        {
            Room create;
            var room =  RoomGroups.GetOrAdd(roomName, create = new Room(roomName));
            if(create == room)
                room.OnRoomCreateCompleted(new PacketSocketEventArgs()
                {
                    Room = room
                });
            
            return room;
        }

        /// <summary>
        /// Get a room with that name.
        /// </summary>
        /// <param name="roomName">Room name.</param>
        /// <returns>null or room</returns>
        public static Room Get(string roomName)
        {
            RoomGroups.TryGetValue(roomName, out var room);
            return room;
        }

        internal void Join(PacketClient client)
        {
            _clients.Add(client);
        }

        internal void Leave(PacketClient client)
        {
            _clients.Remove(client);
            if (!_clients.IsEmpty) return;
            
            if(RoomGroups.TryRemove(RoomName, out _))
                OnRoomDestroyCompleted(new PacketSocketEventArgs()
                {
                    Room = this
                });
        }

        /// <summary>
        /// It is an event that runs when a room is created.
        /// </summary>
        /// <param name="e">EventHandler</param>
        public static void RegisterRoomCreateEvent(EventHandler<PacketSocketEventArgs> e)
        {
            RoomCreateCompleted += e;
        }

        /// <summary>
        /// This is an event that runs when a room is deleted.
        /// </summary>
        /// <param name="e">EventHandler</param>
        public static void RegisterRoomDestroyEvent(EventHandler<PacketSocketEventArgs> e)
        {
            RoomDestroyCompleted += e;
        }

        protected virtual void OnRoomCreateCompleted(PacketSocketEventArgs e)
        {
            RoomCreateCompleted?.Invoke(this, e);
        }

        protected virtual void OnRoomDestroyCompleted(PacketSocketEventArgs e)
        {
            RoomDestroyCompleted?.Invoke(this, e);
        }
    }
}