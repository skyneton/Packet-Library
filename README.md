# EASY Socket Packet Post C#
## Sample Code
### Server
```csharp
SocketPacket.PacketSocket socket = new SocketPacket.PacketSocket(
    System.Net.Sockets.AddressFamily.InterNetwork,
    System.Net.Sockets.SocketType.Stream,
    System.Net.Sockets.ProtocolType.Tcp);

socket.Bind(new IPEndPoint(IPAddress.Any, PORT));
```
### Client
```csharp
SocketPacket.PacketSocket socket = new SocketPacket.PacketSocket(
    System.Net.Sockets.AddressFamily.InterNetwork,
    System.Net.Sockets.SocketType.Stream,
    System.Net.Sockets.ProtocolType.Tcp);

socket.ConnectTimeout(new IPEndPoint(IPAddress, PORT), milliseconds);
// If the connection fails after milliseconds, interrupt the connection.

// socket.Connect(new IPEndPoint(IPAddress, PORT));
```

### Event
```csharp
socket.AcceptCompleted += new EventHandler<PacketSocketAsyncEventArgs>(object sender, PacketSocketAsyncEventArgs e);
// Called when a socket connects to the server.
// e.AcceptSocket <- Client Socket

socket.ConnectCompleted += new EventHandler<PacketSocketAsyncEventArgs>(object sender, PacketSocketAsyncEventArgs e);
// Called when the client contacts the server.
// e.ConnectSocket <- Connected Socket

socket.ReceiveCompleted += new EventHandler<PacketSocketAsyncEventArgs>(object sender, PacketSocketAsyncEventArgs e);
// Called when a packet is received.
// e.ReceivePacket <- Received Packet

socket.DisconnectCompleted += new EventHandler<PacketSocketAsyncEventArgs>(object sender, PacketSocketAsyncEventArgs e);
// Called when socket disconnects.
// e.DisconnectSocket <- Disconnected Socket
```

## How to generate custom packets?
1. Open SocketPacket Project
2. Create csharp Class in Network folder
3. Write
>```Csharp
>//sample code
>namespace SocketPacket.Network {
>    [Serializable]
>    public class SamplePacket : Packet {
>        public string data;
>    }
>}
>```
4. build project
5. Apply libraries to target projects