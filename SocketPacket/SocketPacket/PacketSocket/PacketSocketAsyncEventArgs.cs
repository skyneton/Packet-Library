using SocketPacket.Network;
using System;

namespace SocketPacket.PacketSocket {
    public class PacketSocketAsyncEventArgs : EventArgs {
        /// <summary>
        /// AcceptCompleted 이벤트에서 수락된 Socket을 가져옵니다.
        /// </summary>
        public PacketSocket AcceptSocket { get; internal set; }
        /// <summary>
        /// ConnectCompleted 이벤트에서 연결된 소켓을 가져옵니다.
        /// </summary>
        public PacketSocket ConnectSocket { get; internal set; }
        /// <summary>
        /// DisconnectCompleted 이벤트에서 중단된 소켓을 가져옵니다.
        /// </summary>
        public PacketSocket DisconnectSocket { get; internal set; }
        /// <summary>
        /// ReceiveCompleted 이벤트에서 패킷을 보낸 소켓을 가져옵니다.
        /// </summary>
        public PacketSocket ReceiveSocket { get; internal set; }
        /// <summary>
        /// ReceiveCompleted 이벤트에서 패킷을 가져옵니다.
        /// <para>패킷 버퍼에 패킷이 남아있습니다.</para>
        /// </summary>
        public Packet ReceivePacket { get; internal set; }
        /// <summary>
        /// ReceiveCompleted 이벤트에서 패킷 버퍼에 있는 패킷의 수를 가져옵니다.
        /// </summary>
        public int ReceivePacketAmount { get; internal set; }
    }
}
