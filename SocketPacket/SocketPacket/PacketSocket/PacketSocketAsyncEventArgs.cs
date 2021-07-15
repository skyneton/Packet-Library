using SocketPacket.Network;
using System;

namespace SocketPacket.PacketSocket {
    public class PacketSocketAsyncEventArgs : EventArgs {
        public PacketSocket AcceptSocket { get; internal set; }
        public PacketSocket ConnectSocket { get; internal set; }
        public PacketSocket DisconnectSocket { get; internal set; }
        public PacketSocket ReceiveSocket { get; internal set; }
        public Packet ReceivePacket { get; internal set; }
        public int ReceivePacketAmount { get; internal set; }
    }
}
