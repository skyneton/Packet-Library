using System;

namespace SocketPacket.Network {
    [Serializable]
    public class BytePacket : Packet {
        public byte[] data;
        public BytePacket() { }
        public BytePacket(byte[] data) {
            this.data = data;
        }
    }
}
