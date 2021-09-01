using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SocketPacket.Network {
    [Serializable]
    public class Packet {
        public int type = 0;
        public Packet() {
            type = 0;
        }
        public static byte[] Serialize(Packet packet) {
            MemoryStream ms = new MemoryStream();

            BinaryFormatter bf = new BinaryFormatter();
            //bf.Binder = new PacketBinder();

            bf.Serialize(ms, packet);

            byte[] result = ms.ToArray();
            ms.Dispose();

            return result;
        }
        public static byte[] Serialize(object packet) {
            MemoryStream ms = new MemoryStream();

            BinaryFormatter bf = new BinaryFormatter();
            //bf.Binder = new PacketBinder();

            bf.Serialize(ms, packet);

            byte[] result = ms.ToArray();
            ms.Dispose();

            return result;
        }
        public static object Deserialize(byte[] data) {
            MemoryStream ms = new MemoryStream(data);

            BinaryFormatter bf = new BinaryFormatter();
            //bf.Binder = new PacketBinder();

            object result = bf.Deserialize(ms);

            ms.Dispose();
            return result;
        }
    }
}
