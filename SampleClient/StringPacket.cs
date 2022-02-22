using PacketSocket.Network;
using PacketSocket.Utils;

namespace SampleClient
{
    public class StringPacket : IPacket
    {
        public string Data;
        
        public int PacketKey => 0;
        public void Write(ByteBuf buf)
        {
            buf.WriteString(Data);
        }

        public void Read(ByteBuf buf)
        {
            Data = buf.ReadString();
        }
    }
}