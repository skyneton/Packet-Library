using System;
using SocketPacket.Vector;

namespace SocketPacket.Network {

    [Serializable]
    public class Vector2DPacket : Packet {
        public Vector2D[] data;
        public Vector2DPacket() { }

        public Vector2DPacket(Vector2D[] data) {
            this.data = data;
        }
    }

    [Serializable]
    public class Vector3DPacket : Packet {
        public Vector3D[] data;

        public Vector3DPacket() { }
        public Vector3DPacket(Vector3D[] data) {
            this.data = data;
        }
    }
}
