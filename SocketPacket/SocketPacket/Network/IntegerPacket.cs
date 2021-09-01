using System;

namespace SocketPacket.Network {
    [Serializable]
    public class IntegerPacket : Packet {
        public int[] data;
        public IntegerPacket() { }
        public IntegerPacket(int[] data) {
            this.data = data;
        }
    }
}
