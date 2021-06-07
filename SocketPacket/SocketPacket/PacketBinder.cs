using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace SocketPacket {
    public class PacketBinder : SerializationBinder {
        public static string assem;
        public override Type BindToType(string assemblyName, string typeName) {
            assemblyName = Assembly.GetExecutingAssembly().FullName;
            assem = assemblyName;

            return Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));
        }
    }
}
