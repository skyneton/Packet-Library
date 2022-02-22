using System.Collections.Concurrent;
using PacketSocket.Utils;

namespace PacketSocket.Network
{
    public static class PacketManager
    {
        private static readonly ConcurrentDictionary<int, IPacket> _packets = new();

        /// <summary>
        /// Registering packets.
        /// </summary>
        /// <param name="packet">Your packet.</param>
        public static void RegisterPacket(IPacket packet)
        {
            _packets.TryAdd(packet.PacketKey, packet);
        }

        internal static IPacket Handle(ByteBuf buf)
        {
            if (!_packets.TryGetValue(buf.ReadVarInt(), out var packet)) return null;
            packet.Read(buf);
            
            return packet;
        }
    }
}