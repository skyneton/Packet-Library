# EASY Socket Packet Post C#
## Sample Code
### Server
```csharp
var socket = new PacketSocket.Network.Sockets.PacketListener(port);
socket.Start();
```
### Client
```csharp
var socket = new PacketSocket.Network.Sockets.PacketClient();
socket.ConnectTimeout(hostname, port, milliseconds);
// If the connection fails after milliseconds, interrupt the connection.

// socket.Connect(new IPEndPoint(address, port));
```

### Event
```csharp
PacketListener.RegisterAcceptEvent(new EventHandler<PacketSocketEventArgs>(object sender, PacketSocketEventArgs e));
// Get the connected PacketClient from PacketListener.
// e.AcceptClient

PacketClient.RegisterConnectEvent(new EventHandler<PacketSocketEventArgs>(object sender, PacketSocketEventArgs e));
// Get the PacketClient connected to the server.
// e.ConnectClient

Socket.RegisterReceiveEvent(new EventHandler<PacketSocketEventArgs>(object sender, PacketSocketEventArgs e));
// Get the PacketClient that received the packet.
// e.ReceivePacket
// e.ReceiveClient

Socket.RegisterDisconnectEvent(new EventHandler<PacketSocketEventArgs>(object sender, PacketSocketEventArgs e));
// Get the PacketClient where the server connection has been interrupted.
// e.DisconnectClient
```

## How to generate custom packets?
>```Csharp
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
>
>PacketManager.RegisterPacket(new SamplePacket()); // important
>```
4. build project
5. Apply libraries to target projects

# Example
- [Server](https://github.com/skyneton/SocketPacket/blob/main/SampleServer/Program.cs)
- [Client](https://github.com/skyneton/SocketPacket/blob/main/SampleClient/Program.cs)