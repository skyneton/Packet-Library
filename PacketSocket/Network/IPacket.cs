using PacketSocket.Utils;

namespace PacketSocket.Network
{
    public interface IPacket
    {
        /// <summary>
        /// This is the key used to distinguish packets.
        /// </summary>
        /// <returns>Primary key.</returns>
        public int PacketKey { get; }
        /// <summary>
        /// Convert variables to byte arrays.
        /// </summary>
        /// <param name="buf">Write down the variables.</param>
        public void Write(ByteBuf buf);
        /// <summary>
        /// Convert byte arrays to variables.
        /// </summary>
        /// <param name="buf">Read the variables.</param>
        public void Read(ByteBuf buf);
    }
}