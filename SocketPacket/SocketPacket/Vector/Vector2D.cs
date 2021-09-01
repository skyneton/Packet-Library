using System;

namespace SocketPacket.Vector {
    [Serializable]
    public class Vector2D {
        public float x, y;

        public static float Distance(Vector2D vec1, Vector2D vec2) {
            return (float) Math.Sqrt(Math.Pow((double) (vec1.x - vec2.x), 2) + Math.Pow((double) (vec1.y - vec2.y), 2));
        }
    }
}
