using SocketPacket.Network;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SocketPacket.PacketSocket {
    public class PacketSocket : IDisposable {
        private Socket socket;
        public bool NoDelay { get { return socket.NoDelay; } set { socket.NoDelay = value; } }
        public int SendBufferSize { get { return socket.SendBufferSize; } set { socket.SendBufferSize = value; } }
        public int SendTimeout { get { return socket.SendTimeout; } set { socket.SendTimeout = value; } }
        public int ReceiveTimeout { get { return socket.ReceiveTimeout; } set { socket.ReceiveTimeout = value; } }
        public bool IsBound { get { return socket.IsBound; } }
        public bool Connected { get { return socket.Connected; } }
        public EndPoint RemoteEndPoint { get { return socket.RemoteEndPoint; } }

        private List<PacketSocket> clientSocketList;
        private Thread acceptSocketThread, packetReadThread, clientCleanThread, clientDisconnectThread;
        private bool isRunnable;
        private bool disposedValue;

        public event EventHandler<PacketSocketAsyncEventArgs> AcceptCompleted, ConnectCompleted, DisconnectCompleted, ReceiveCompleted;

        private PacketSocket(Socket socket) {
            this.socket = socket;
        }

        public PacketSocket(SocketInformation socketInformation) {
            socket = new Socket(socketInformation);
        }

        public PacketSocket(SocketType socketType, ProtocolType protocolType) {
            socket = new Socket(socketType, protocolType);
        }

        public PacketSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) {
            socket = new Socket(addressFamily, socketType, protocolType);
        }

        public void Connect(EndPoint remoteEP) {
            socket.Connect(remoteEP);

            Close(false);
            isRunnable = true;
            RunPacketReadInClientWorker();
            RunClientDisconnectWorker();

            if (ConnectCompleted != null) {
                PacketSocketAsyncEventArgs args = new PacketSocketAsyncEventArgs();
                args.ConnectSocket = this;
                ConnectCompleted(this, args);
            }
        }

        public void Connect(IPAddress address, int port) {
            socket.Connect(address, port);

            Close(false);
            isRunnable = true;
            RunPacketReadInClientWorker();
            RunClientDisconnectWorker();

            if (ConnectCompleted != null) {
                PacketSocketAsyncEventArgs args = new PacketSocketAsyncEventArgs();
                args.ConnectSocket = this;
                ConnectCompleted(this, args);
            }
        }

        public void Connect(string host, int port) {
            socket.Connect(host, port);

            Close(false);
            isRunnable = true;
            RunPacketReadInClientWorker();
            RunClientDisconnectWorker();

            if (ConnectCompleted != null) {
                PacketSocketAsyncEventArgs args = new PacketSocketAsyncEventArgs();
                args.ConnectSocket = this;
                ConnectCompleted(this, args);
            }
        }

        public Task ConnectTimeout(IPEndPoint remoteEP, int timeout, bool force = false) {
            return Task.Run(() => {
                if ((socket.IsBound || socket.Connected) && !force) return;
                IAsyncResult result = socket.BeginConnect(remoteEP, null, null);
                bool connected = result.AsyncWaitHandle.WaitOne(timeout, true);

                try {
                    socket.EndConnect(result);
                    if (connected) {
                        isRunnable = true;

                        Close(false);
                        isRunnable = true;
                        RunPacketReadInClientWorker();
                        RunClientDisconnectWorker();

                        if (ConnectCompleted != null) {
                            PacketSocketAsyncEventArgs args = new PacketSocketAsyncEventArgs();
                            args.ConnectSocket = this;
                            ConnectCompleted(this, args);
                        }
                    }
                }
                catch { }
            });
        }

        public Task ConnectTimeout(IPAddress address, int port, int timeout, bool force = false) {
            return Task.Run(() => {
                if ((socket.IsBound || socket.Connected) && !force) return;
                IAsyncResult result = socket.BeginConnect(address, port, null, null);
                bool connected = result.AsyncWaitHandle.WaitOne(timeout, true);

                try {
                    socket.EndConnect(result);
                    if (connected) {
                        isRunnable = true;

                        Close(false);
                        isRunnable = true;
                        RunPacketReadInClientWorker();
                        RunClientDisconnectWorker();

                        if (ConnectCompleted != null) {
                            PacketSocketAsyncEventArgs args = new PacketSocketAsyncEventArgs();
                            args.ConnectSocket = this;
                            ConnectCompleted(this, args);
                        }
                    }
                }
                catch { }
            });
        }

        public Task ConnectTimeout(IPAddress[] addresses, int port, int timeout, bool force = false) {
            return Task.Run(() => {
                if ((socket.IsBound || socket.Connected) && !force) return;
                IAsyncResult result = socket.BeginConnect(addresses, port, null, null);
                bool connected = result.AsyncWaitHandle.WaitOne(timeout, true);

                try {
                    socket.EndConnect(result);
                    if (connected) {
                        isRunnable = true;

                        Close(false);
                        isRunnable = true;
                        RunPacketReadInClientWorker();
                        RunClientDisconnectWorker();

                        if (ConnectCompleted != null) {
                            PacketSocketAsyncEventArgs args = new PacketSocketAsyncEventArgs();
                            args.ConnectSocket = this;
                            ConnectCompleted(this, args);
                        }
                    }
                }
                catch { }
            });
        }

        public Task ConnectTimeout(string host, int port, int timeout, bool force = false) {
            return Task.Run(() => {
                if ((socket.IsBound || socket.Connected) && !force) return;
                IAsyncResult result = socket.BeginConnect(host, port, null, null);
                bool connected = result.AsyncWaitHandle.WaitOne(timeout, true);

                try {
                    socket.EndConnect(result);
                    if (connected) {
                        isRunnable = true;

                        Close(false);
                        isRunnable = true;
                        RunPacketReadInClientWorker();
                        RunClientDisconnectWorker();

                        if (ConnectCompleted != null) {
                            PacketSocketAsyncEventArgs args = new PacketSocketAsyncEventArgs();
                            args.ConnectSocket = this;
                            ConnectCompleted(this, args);
                        }
                    }
                }
                catch { }
            });
        }

        public void Send(Packet packet) {
            if (packet == null) return;
            try {
                byte[] buf = Packet.Serialize(packet);
                if (buf == null) return;

                socket.Send(BitConverter.GetBytes(buf.Length));
                socket.Send(buf, buf.Length, SocketFlags.None);
            }
            catch(Exception e) {
                throw e;
            }
        }

        public void Bind(EndPoint localEP) {
            socket.Bind(localEP);
            socket.Listen(50);

            Close(false);

            isRunnable = true;
            clientSocketList = new List<PacketSocket>();

            acceptSocketThread = new Thread(() => AcceptSocketWorker());
            acceptSocketThread.Start();
            clientCleanThread = new Thread(() => ClientCleanWorker());
            clientCleanThread.Start();
            RunPacketReadInServerWorker();
        }

        private void CloseThread(Thread thread) {
            if (thread != null && thread.IsAlive) {
                try {
                    thread.Abort();
                }
                catch { }
            }
        }

        private void AcceptSocketWorker() {
            while(isRunnable && socket.IsBound) {
                try {
                    Socket client = socket.Accept();
                    PacketSocket packetClient = new PacketSocket(client);
                    clientSocketList.Add(packetClient);
                    if(AcceptCompleted != null) {
                        PacketSocketAsyncEventArgs args = new PacketSocketAsyncEventArgs();
                        args.AcceptSocket = packetClient;
                        AcceptCompleted(this, args);
                    }
                }
                catch { }
            }
        }

        private void RunPacketReadInClientWorker() {
            packetReadThread = new Thread(() => PacketReadWorkerInClient());
            packetReadThread.Start();
        }

        private void RunPacketReadInServerWorker() {
            packetReadThread = new Thread(() => PacketReadWorkerInServer());
            packetReadThread.Start();
        }

        private void PacketReadWorkerInClient() {
            while(isRunnable && socket != null && socket.Connected) {
                PacketReceived();
            }
        }

        private void PacketReadWorkerInServer() {
            while(isRunnable) {
                for(int i = 0; i < clientSocketList.Count; i++) {
                    try {
                        PacketSocket client = clientSocketList[i];
                        if (client.socket != null && client.socket.Connected) {
                            client.PacketReceived();
                        }
                    }
                    catch { }
                }
            }
        }

        private void TrySocketClose(Socket socket) {
            try {
                socket.Close();
            }
            catch { }
        }

        private void ClientCleanWorker() {
            while(isRunnable) {
                int i = 0;
                while(i < clientSocketList.Count) {
                    try {
                        PacketSocket client = clientSocketList[i];
                        if (client.socket == null || !client.socket.Connected) {
                            TrySocketClose(client.socket);
                            clientSocketList.RemoveAt(i);

                            if (DisconnectCompleted != null) {
                                PacketSocketAsyncEventArgs args = new PacketSocketAsyncEventArgs();
                                args.DisconnectSocket = client;
                                DisconnectCompleted(this, args);
                            }
                            continue;
                        }
                    }
                    catch { }
                    i++;
                }
            }
        }

        private void RunClientDisconnectWorker() {
            clientDisconnectThread = new Thread(() => ClientDisconnectWorker());
            clientDisconnectThread.Start();
        }

        private void ClientDisconnectWorker() {
            while (isRunnable && socket.Connected);
            if (DisconnectCompleted != null) {
                PacketSocketAsyncEventArgs args = new PacketSocketAsyncEventArgs();
                args.DisconnectSocket = this;
                DisconnectCompleted(this, args);
            }
        }

        private void PacketReceived() {
            try {
                while(socket.Available > 4) {
                    byte[] buf = new byte[4];
                    socket.Receive(buf, SocketFlags.Peek);

                    int length = BitConverter.ToInt32(buf, 0);
                    if (length > socket.Available - 4) break;

                    socket.Receive(buf);

                    buf = new byte[length];
                    socket.Receive(buf, buf.Length, SocketFlags.None);

                    if (ReceiveCompleted != null) {
                        PacketSocketAsyncEventArgs args = new PacketSocketAsyncEventArgs();
                        args.ReceiveSocket = this;
                        args.ReceivePacket = Packet.Deserialize(buf);
                        ReceiveCompleted(this, args);
                    }
                }
            }catch(Exception e) {
                Console.WriteLine(e);
                try {
                    if (socket != null && socket.Connected && socket.Available > 0) {
                        byte[] buf = new byte[socket.Available];
                        socket.Receive(buf);
                    }
                }
                catch { }
            }
        }

        private void Close(bool socketClose) {
            try {
                if (socketClose) socket.Close();
            }
            catch { }

            isRunnable = false;

            CloseThread(acceptSocketThread);
            CloseThread(packetReadThread);
            CloseThread(clientCleanThread);
            CloseThread(clientDisconnectThread);

            if (clientSocketList != null) {
                for (int i = 0; i < clientSocketList.Count; i++) {
                    clientSocketList[i].socket.Close();
                }
            }
        }

        public void Close() {
            Close(false);
            socket.Close();
        }

        public void Disconnect(bool reuseSocket) {
            socket.Disconnect(reuseSocket);
            Close(false);
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                Close(true);

                disposedValue = true;
            }
        }

        void IDisposable.Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
