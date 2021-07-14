using SocketPacket.Network;
using System;

namespace SocketPacket.PacketSocket {
    public class PacketSocketAsyncEventArgs : EventArgs {
        public PacketSocket AcceptSocket { get; internal set; }
        public PacketSocket DisconnectSocket { get; internal set; }
        public Packet ReceivePacket { get; internal set; }
    }
}
