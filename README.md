# EASY Socket Packet Post C#
# Sample Code
## Server
>```cs
>var socket = new PacketSocket.Network.Sockets.PacketListener(port);
>socket.Start();
>```
## Client
>```cs
>var socket = new PacketSocket.Network.Sockets.PacketClient();
>socket.ConnectTimeout(hostname, port, milliseconds);
>// If the connection fails after milliseconds, interrupt the connection.
>
>// socket.Connect(new IPEndPoint(address, port));
>```

## Event
>```cs
>PacketListener.RegisterAcceptEvent(new EventHandler<PacketSocketEventArgs>(object sender, PacketSocketEventArgs e));
>// Get the connected PacketClient from PacketListener.
>// e.AcceptClient
>
>PacketClient.RegisterConnectEvent(new EventHandler<PacketSocketEventArgs>(object >sender, PacketSocketEventArgs e));
>// Get the PacketClient connected to the server.
>// e.ConnectClient
>
>Socket.RegisterReceiveEvent(new EventHandler<PacketSocketEventArgs>(object sender, PacketSocketEventArgs e));
>// Get the PacketClient that received the packet.
>// e.ReceivePacket
>// e.ReceiveClient
>
>Socket.RegisterDisconnectEvent(new EventHandler<PacketSocketEventArgs>(object sender, PacketSocketEventArgs e));
>// Get the PacketClient where the server connection has been interrupted.
>// e.DisconnectClient
>```

## Room
>```cs
>PacketClient.Join(roomName);
>PacketClient.To(roomName)
>```

# How to generate custom packets?
>```cs
>//sample code
>public class SamplePacket : IPacket {
>    public string Data1;
>    public int Data2;
>
>    public int PacketKey => 0; // primary key
>    public void Write(ByteBuf buf) {
>        buf.WriteString(Data1);
>        buf.WriteVarInt(Data2);
>    }
>    public void Read(ByteBuf buf) {
>        Data1 = buf.ReadString();
>        Data2 = buf.ReadVarInt();
>    }
>}
>```
>```cs
>PacketManager.RegisterPacket(new SamplePacket()); // important
>```

# Example
- [Server](./SampleServer/Program.cs)
- [Client](./SampleClient/Program.cs)