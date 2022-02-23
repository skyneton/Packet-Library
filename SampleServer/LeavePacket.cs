using PacketSocket.Network;
using PacketSocket.Utils;

namespace SampleServer
{
    public class LeavePacket : IPacket
    {
        public string RoomName;
        public int PacketKey => 2;
        public void Write(ByteBuf buf)
        {
            buf.WriteString(RoomName);
        }

        public void Read(ByteBuf buf)
        {
            RoomName = buf.ReadString();
        }
    }
}