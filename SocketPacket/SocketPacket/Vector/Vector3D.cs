using System;

namespace SocketPacket.Vector {
    [Serializable]
    public class Vector3D {
        public float x, y, z;
        public static float Distance(Vector3D vec1, Vector3D vec2) {
            return (float) Math.Sqrt(Math.Pow((double)(vec1.x - vec2.x), 2) + Math.Pow((double)(vec1.y - vec2.y), 2) + Math.Pow((double)(vec1.z - vec2.z), 2));
        }
    }
}
