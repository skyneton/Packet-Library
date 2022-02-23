using PacketSocket.Network;
using PacketSocket.Utils;

namespace SampleClient
{
    public class RoomPacket : IPacket
    {
        public string RoomName;
        public int PacketKey => 1;
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