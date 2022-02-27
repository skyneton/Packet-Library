using System;
using PacketSocket.Network.Sockets;

namespace PacketSocket.Network.Event
{
    public class PacketSocketEventArgs : EventArgs
    {
        /// <summary>
        /// Get the connected PacketClient from PacketListener.
        /// </summary>
        public PacketClient AcceptClient { get; internal init; }
        /// <summary>
        /// Get the PacketClient connected to the server.
        /// </summary>
        public PacketClient ConnectClient { get; internal init; }
        /// <summary>
        /// Get the PacketClient where the server connection has been interrupted.
        /// </summary>
        public PacketClient DisconnectClient { get; internal init; }
        /// <summary>
        /// Get the PacketClient that received the packet.
        /// </summary>
        public PacketClient ReceiveClient { get; internal init; }
        /// <summary>
        /// Get the packet you received.
        /// </summary>
        public IPacket ReceivePacket { get; internal init; }
        
        /// <summary>
        /// Get the created or deleted room.
        /// </summary>
        public Room Room { get; internal init; }
    }
}