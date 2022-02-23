using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using PacketSocket.Network.Event;
using PacketSocket.Utils;

namespace PacketSocket.Network.Sockets
{
    public class PacketListener
    {
        private readonly ThreadFactory _threadFactory = new();
        private readonly TcpListener _listener;
        public Socket Server => _listener?.Server;

        private readonly ConcurrentBag<PacketClient> _clients = new();
        private readonly ConcurrentQueue<PacketClient> _destroyClients = new();

        public ReadOnlyCollection<PacketClient> Clients => new(_clients.ToList());

        private bool _isRunnable;
        
        private bool IsActive => (_listener?.Server.IsBound ?? false) && _isRunnable;
        public EndPoint LocalEndpoint => _listener?.LocalEndpoint;

        public bool ExclusiveAddressUse
        {
            get => _listener.ExclusiveAddressUse;
            set => _listener.ExclusiveAddressUse = value;
        }

        private event EventHandler<PacketSocketEventArgs> AcceptCompleted;
        private event EventHandler<PacketSocketEventArgs> DisconnectCompleted;
        
        public PacketListener(IPAddress localaddr, int port) => _listener = new TcpListener(localaddr, port);
        public PacketListener(IPEndPoint localEP) => _listener = new TcpListener(localEP);
        public PacketListener(int port) => _listener = new TcpListener(IPAddress.Any, port);

        public void Start()
        {
            _listener.Start();
            Init();
        }
        
        public void Start(int backlog)
        {
            _listener.Start(backlog);
            Init();
        }

        private void Init()
        {
            _isRunnable = true;
            _threadFactory.LaunchThread(new Thread(AcceptWorker));
            _threadFactory.LaunchThread(new Thread(UpdateWorker));
            _threadFactory.LaunchThread(new Thread(DestroyWorker));
        }

        public void Stop()
        {
            _isRunnable = false;
            
            _threadFactory.KillAll();
            _listener.Stop();
        }

        /// <summary>
        /// Register events that occur when PacketClient accesses.
        /// </summary>
        /// <param name="e">EventHandler</param>
        public void RegisterAcceptEvent(EventHandler<PacketSocketEventArgs> e)
        {
            AcceptCompleted += e;
        }

        /// <summary>
        /// PacketClient registers events that occur at the end of the connection.
        /// </summary>
        /// <param name="e">EventHandler</param>
        public void RegisterDisconnectEvent(EventHandler<PacketSocketEventArgs> e)
        {
            DisconnectCompleted += e;
        }

        protected virtual void OnAcceptCompleted(PacketSocketEventArgs e)
        {
            AcceptCompleted?.Invoke(this, e);
        }

        protected virtual void OnDisconnectCompleted(PacketSocketEventArgs e)
        {
            DisconnectCompleted?.Invoke(this, e);
        }

        private void AcceptWorker()
        {
            while (IsActive)
            {
                try
                {
                    var client = new PacketClient(_listener.AcceptTcpClient());
                    OnAcceptCompleted(new PacketSocketEventArgs
                    {
                        AcceptClient = client
                    });
                    
                    _clients.Add(client);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        private void UpdateWorker()
        {
            while (IsActive)
            {
                foreach (var client in _clients)
                {
                    if (!(client?.IsAvailable ?? false))
                    {
                        if (client != null && !client.DestroyEnqueued)
                        {
                            client.DestroyEnqueued = true;
                            _destroyClients.Enqueue(client);
                        }

                        continue;
                    }
                    
                    client.Update();
                }
            }
        }

        private void DestroyWorker()
        {
            while (IsActive)
            {
                while (!_destroyClients.IsEmpty)
                {
                    if(!_destroyClients.TryDequeue(out var client)) continue;

                    OnDisconnectCompleted(new PacketSocketEventArgs()
                    {
                        DisconnectClient = client
                    });
                    
                    client.Close();
                    _clients.Remove(client);
                }
            }
        }

        /// <summary>
        /// It sends packets to all connected clients.
        /// </summary>
        /// <param name="packet">Your packet.</param>
        public void Broadcast(IPacket packet)
        {
            var buf = new ByteBuf();
            buf.WriteVarInt(packet.PacketKey);
            packet.Write(buf);

            var data = buf.Flush();
            
            foreach (var client in _clients)
            {
                client.SendByteArray(data);
            }
        }
    }
}