using System;

namespace SocketPacket.Network {
    [Serializable]
    public class StringPacket : Packet {
        public string[] data;

        public StringPacket() { }
        public StringPacket(string[] data) {
            this.data = data;
        }
    }
}
