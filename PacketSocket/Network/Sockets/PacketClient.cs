using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using PacketSocket.Network.Event;
using PacketSocket.Utils;

namespace PacketSocket.Network.Sockets
{
    public class PacketClient
    {
        private readonly ThreadFactory _factory = new();
        /// <summary>
        /// Output the error log to the console.
        /// </summary>
        public static bool PrintErrorLog = true;

        /// <summary>
        /// When Timeout is set and Keep Alive Packet is specified, packets are sent according to the corresponding ratio.
        /// </summary>
        public static float KeepAlivePercent = .7F;
        
        private readonly TcpClient _client;
        private readonly NetworkBuf _networkBuf = new();

        internal bool DestroyEnqueued;

        private readonly ConcurrentDictionary<string, Room> _rooms = new();

        public ReadOnlyCollection<Room> Rooms => new(_rooms.Values.ToList());

        private static IPacket _keepAlivePacket;

        /// <summary>
        /// Gets or sets the underlying Socket.
        /// </summary>
        public Socket Client
        {
            get => _client?.Client;
            set => _client.Client = value;
        }
        
        public bool NoDelay
        {
            get => _client.NoDelay;
            set => _client.NoDelay = value;
        }

        public bool IsAvailable { get; private set; } = true;
        public bool Connected => _client.Connected;

        /// <summary>
        /// Specifies the maximum milliseconds without packets.
        /// </summary>
        public int Timeout { get; set; } = 0;
        
        public long LastPacketMillis { get; private set; } = TimeManager.CurrentTimeMillis;

        private event EventHandler<PacketSocketEventArgs> ConnectCompleted;
        private event EventHandler<PacketSocketEventArgs> ReceiveCompleted;
        private event EventHandler<PacketSocketEventArgs> DisconnectCompleted;
        
        internal PacketClient(TcpClient client) => _client = client;

        public PacketClient() => _client = new TcpClient();
        public PacketClient(AddressFamily family) => _client = new TcpClient(family);

        public PacketClient(IPEndPoint endPoint)
        {
            _client = new TcpClient(endPoint);
            InitConnect();
        }

        public PacketClient(string hostname, int port)
        {
            _client = new TcpClient(hostname, port);
            InitConnect();
        }

        private void InitConnect()
        {
            OnConnectCompleted(new PacketSocketEventArgs()
            {
                ConnectClient = this
            });

            LastPacketMillis = TimeManager.CurrentTimeMillis;
            _factory.LaunchThread(new Thread(UpdateWorker));
        }

        public void Connect(IPAddress address, int port)
        {
            _client.Connect(address, port);
            InitConnect();
        }

        public void Connect(IPAddress[] ipAddresses, int port)
        {
            _client.Connect(ipAddresses, port);
            InitConnect();
        }

        public void Connect(IPEndPoint ipEndPoint)
        {
            _client.Connect(ipEndPoint);
            InitConnect();
        }

        public void Connect(string hostname, int port)
        {
            _client.Connect(hostname, port);
            InitConnect();
        }

        /// <summary>
        /// It's asynchronous connection.
        /// </summary>
        /// <param name="hostname">The host to connect to.</param>
        /// <param name="port">Port to connect to.</param>
        /// <param name="timeout">Maximum time to wait for access.</param>
        /// <returns>Check if it's connected.</returns>
        public Task<bool> ConnectTimeout(string hostname, int port, int timeout)
        {
            return Task.Run(() =>
            {
                var result = _client.BeginConnect(hostname, port, null, null);
                var connected = result.AsyncWaitHandle.WaitOne(timeout, true);
                try
                {
                    _client.EndConnect(result);
                    if (connected)
                    {
                        InitConnect();
                    }
                    return connected;
                }
                catch (Exception e)
                {
                    if(PrintErrorLog) Console.WriteLine(e);
                }

                return false;
            });
        }

        private void Disconnect()
        {
            IsAvailable = false;
            _factory.KillAll();
            
            OnDisconnectCompleted(new PacketSocketEventArgs()
            {
                DisconnectClient = this
            });
            
            foreach (var room in _rooms.Values)
            {
                room.Leave(this);
            }
            
            _rooms.Clear();
        }

        public void Close()
        {
            if(IsAvailable)
                Disconnect();

            try
            {
                _client.Close();
                _client.Dispose();
            }
            catch (Exception e)
            {
                if(PrintErrorLog) Console.WriteLine(e);
            }
        }

        /// <summary>
        /// PacketClient registers events that occur when accessing the server.
        /// </summary>
        /// <param name="e">EventHandler</param>
        public void RegisterConnectEvent(EventHandler<PacketSocketEventArgs> e)
        {
            ConnectCompleted += e;
        }

        /// <summary>
        /// Register events that occur when PacketClient receives packets.
        /// </summary>
        /// <param name="e">EventHandler</param>
        public void RegisterReceiveEvent(EventHandler<PacketSocketEventArgs> e)
        {
            ReceiveCompleted += e;
        }

        /// <summary>
        /// PacketClient registers events that occur at the end of the connection.
        /// </summary>
        /// <param name="e">EventHandler</param>
        public void RegisterDisconnectEvent(EventHandler<PacketSocketEventArgs> e)
        {
            DisconnectCompleted += e;
        }

        protected virtual void OnConnectCompleted(PacketSocketEventArgs e)
        {
            ConnectCompleted?.Invoke(this, e);
        }

        protected virtual void OnReceiveCompleted(PacketSocketEventArgs e)
        {
            ReceiveCompleted?.Invoke(this, e);
        }

        protected virtual void OnDisconnectCompleted(PacketSocketEventArgs e)
        {
            DisconnectCompleted?.Invoke(this, e);
        }

        /// <summary>
        /// After setting Timeout, set the packet to be sent at each specified cost.
        /// </summary>
        /// <param name="packet">Your packet.</param>
        public static void KeepAlivePacket(IPacket packet)
        {
            _keepAlivePacket = packet;
        }

        internal void Update()
        {
            if (TimeoutUpdate()) return;
            ReceiveUpdate();
            PacketHandleUpdate();
            KeepAliveUpdate();
        }

        private bool TimeoutUpdate()
        {
            if (_client.Connected && (Timeout <= 0 || TimeManager.CurrentTimeMillis - LastPacketMillis <= Timeout)) return false;
            Disconnect();
            return true;
        }

        private void ReceiveUpdate()
        {
            if (_client.Available <= 0) return;

            LastPacketMillis = TimeManager.CurrentTimeMillis;

            _networkBuf.Buf ??= new byte[ByteBuf.ReadVarInt(_client.GetStream())];

            _networkBuf.Offset += _client.GetStream().Read(_networkBuf.Buf, _networkBuf.Offset,
                _networkBuf.Buf.Length - _networkBuf.Offset);
        }

        private void PacketHandleUpdate()
        {
            if(_networkBuf.Buf == null || _networkBuf.Offset != _networkBuf.Buf.Length) return;

            try
            {
                OnReceiveCompleted(new PacketSocketEventArgs()
                {
                    ReceiveClient = this,
                    ReceivePacket = PacketManager.Handle(new ByteBuf(_networkBuf.Buf))
                });
            }
            catch (Exception e)
            {
                if(PrintErrorLog) Console.WriteLine(e);
                Disconnect();
            }

            _networkBuf.Clear();
        }

        private void KeepAliveUpdate()
        {
            if (TimeManager.CurrentTimeMillis - LastPacketMillis < Timeout * KeepAlivePercent || _keepAlivePacket == null) return;
            SendPacket(_keepAlivePacket);
        }

        private void UpdateWorker()
        {
            while (IsAvailable)
            {
                Update();
            }
        }

        public void SendPacket(IPacket packet)
        {
            if(!(IsAvailable && Connected)) return;

            var buf = new ByteBuf();
            buf.WriteVarInt(packet.PacketKey);
            packet.Write(buf);

            var data = buf.Flush();

            try
            {
                _client.GetStream().WriteAsync(data, 0, data.Length);
                _client.GetStream().Flush();

                LastPacketMillis = TimeManager.CurrentTimeMillis;
            }
            catch (Exception)
            {
                Disconnect();
            }
        }

        internal void SendByteArray(byte[] packet)
        {
            if(!(IsAvailable && Connected)) return;

            try
            {
                _client.GetStream().WriteAsync(packet, 0, packet.Length);

                LastPacketMillis = TimeManager.CurrentTimeMillis;
            }
            catch (Exception)
            {
                Disconnect();
            }
        }

        /// <summary>
        /// Access or create a room.
        /// </summary>
        /// <param name="roomName">Room name.</param>
        public void Join(string roomName)
        {
            var room = Room.GetOrAdd(roomName);
            
            if(_rooms.TryAdd(roomName, room))
                room.Join(this);
        }

        /// <summary>
        /// Leave the room.
        /// </summary>
        /// <param name="roomName">Room name.</param>
        public void Leave(string roomName)
        {
            if(_rooms.TryRemove(roomName, out var room))
                room.Leave(this);
        }

        /// <summary>
        /// Get the room where the client is connected.
        /// </summary>
        /// <param name="roomName">Room name.</param>
        /// <returns>null or room</returns>
        public Room To(string roomName)
        {
            _rooms.TryGetValue(roomName, out var room);
            return room;
        }
    }

    internal class NetworkBuf
    {
        public byte[] Buf;
        public int Offset;

        public void Clear()
        {
            Buf = null;
            Offset = 0;
        }
    }
}