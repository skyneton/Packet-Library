using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using PacketSocket.Utils;

namespace PacketSocket.Network.Sockets
{
    public class Room
    {
        public readonly string RoomName;
        
        private static ConcurrentDictionary<string, Room> _rooms = new();
        private ConcurrentBag<PacketClient> _clients = new();

        public static ReadOnlyCollection<Room> Rooms => new(_rooms.Values.ToList());
        public ReadOnlyCollection<PacketClient> Clients => new(_clients.ToList());

        internal Room(string room)
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
            return _rooms.GetOrAdd(roomName, new Room(roomName));
        }

        /// <summary>
        /// Get a room with that name.
        /// </summary>
        /// <param name="roomName">Room name.</param>
        /// <returns>null or room</returns>
        public static Room Get(string roomName)
        {
            _rooms.TryGetValue(roomName, out var room);
            return room;
        }

        internal void Join(PacketClient client)
        {
            _clients.Add(client);
        }

        internal void Leave(PacketClient client)
        {
            _clients.Remove(client);
            if (_clients.IsEmpty)
            {
                _rooms.TryRemove(RoomName, out _);
            }
        }
    }
}