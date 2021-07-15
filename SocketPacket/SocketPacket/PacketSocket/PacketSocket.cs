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

        /// <summary>
        /// 스트림에서 Nagle 알고리즘을 사용할지 여부를 나타냅니다.
        /// </summary>
        public bool NoDelay { get { return socket.NoDelay; } set { socket.NoDelay = value; } }

        /// <summary>
        /// Socket Byte를 보낼 때 최대 버퍼 크기를 지정합니다.
        /// </summary>
        public int SendBufferSize { get { return socket.SendBufferSize; } set { socket.SendBufferSize = value; } }

        /// <summary>
        /// Packet을 보낼 때 최대 지연 시간을 설정합니다.
        /// </summary>
        public int SendTimeout { get { return socket.SendTimeout; } set { socket.SendTimeout = value; } }
        /// <summary>
        /// Packet을 받을 때 최대 지연 시간을 설정합니다.
        /// </summary>
        public int ReceiveTimeout { get { return socket.ReceiveTimeout; } set { socket.ReceiveTimeout = value; } }
        /// <summary>
        /// Socket이 특정 포트에 바인딩 되었는지를 나타냅니다.
        /// </summary>
        public bool IsBound { get { return socket.IsBound; } }
        /// <summary>
        /// Socket이 마지막을 기준으로 연결되었는지를 나타냅니다.
        /// </summary>
        public bool Connected { get { return socket.Connected; } }
        /// <summary>
        /// 원격 연결 포인트를 가져옵니다.
        /// </summary>
        public EndPoint RemoteEndPoint { get { return socket.RemoteEndPoint; } }

        private List<PacketSocket> clientSocketList;
        private Thread acceptSocketThread, packetReadThread, clientCleanThread, clientDisconnectThread;
        private bool isRunnable;
        private bool disposedValue;
        private Queue<Packet> packetQueue = new Queue<Packet>();
        /// <summary>
        /// 서버에 Socket이 연결될경우 호출되는 이벤트입니다.
        /// </summary>
        public event EventHandler<PacketSocketAsyncEventArgs> AcceptCompleted;
        /// <summary>
        /// 클라이언트가 서버에 연결될 경우 호출되는 이벤트입니다.
        /// </summary>
        public event EventHandler<PacketSocketAsyncEventArgs> ConnectCompleted;
        /// <summary>
        /// Socket 연결이 중단될 때 호출되는 이벤트입니다.
        /// </summary>
        public event EventHandler<PacketSocketAsyncEventArgs> DisconnectCompleted;
        /// <summary>
        /// Packet을 받을 때 호출되는 이벤트 입니다.
        /// </summary>
        public event EventHandler<PacketSocketAsyncEventArgs> ReceiveCompleted;

        private PacketSocket(Socket socket) {
            this.socket = socket;
        }

        /// <summary>
        /// Socket을 생성 및 초기화합니다.
        /// </summary>
        public PacketSocket(SocketInformation socketInformation) {
            socket = new Socket(socketInformation);
        }

        /// <summary>
        /// Socket을 생성 및 초기화합니다.
        /// </summary>
        public PacketSocket(SocketType socketType, ProtocolType protocolType) {
            socket = new Socket(socketType, protocolType);
        }

        /// <summary>
        /// Socket을 생성 및 초기화합니다.
        /// </summary>
        public PacketSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) {
            socket = new Socket(addressFamily, socketType, protocolType);
        }

        /// <summary>
        /// Socket을 생성 및 초기화합니다.
        /// </summary>
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

        /// <summary>
        /// 특정 주소에 연결합니다.
        /// </summary>
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

        /// <summary>
        /// 특정 주소에 연결합니다.
        /// </summary>
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

        /// <summary>
        /// 연결을 대기합니다.
        /// </summary>
        /// <param name="timeout">ms 동안 대기할지 설정합니다.</param>
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

        /// <summary>
        /// 연결을 대기합니다.
        /// </summary>
        /// <param name="timeout">ms 동안 대기할지 설정합니다.</param>
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

        /// <summary>
        /// 연결을 대기합니다.
        /// <param name="timeout">ms 동안 대기할지 설정합니다.</param>
        /// </summary>
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

        /// <summary>
        /// 연결을 대기합니다.
        /// </summary>
        /// <param name="timeout">ms 동안 대기할지 설정합니다.</param>
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

        /// <summary>
        /// 주소를 Bind합니다.
        /// </summary>
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

        /// <summary>
        /// Packet을 전송합니다.
        /// </summary>
        public void Send(Packet packet) {
            if (packet == null) return;
            try {
                byte[] buf = Packet.Serialize(packet);
                if (buf == null) return;

                socket.Send(BitConverter.GetBytes(buf.Length));
                socket.Send(buf, buf.Length, SocketFlags.None);
            }
            catch (Exception e) {
                throw e;
            }
        }

        /// <summary>
        /// Packet 버퍼에서 Packet을 가져옵니다.
        /// 버퍼가 비워져있다면 null이 반환됩니다.
        /// </summary>
        public Packet Receive() {
            if (packetQueue.Count <= 0) return null;
            return packetQueue.Dequeue();
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
                        packetQueue.Enqueue(args.ReceivePacket);
                        args.ReceivePacketAmount = packetQueue.Count;
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


        /// <summary>
        /// 소켓을 닫습니다.
        /// </summary>
        public void Close() {
            Close(false);
            socket.Close();
        }

        /// <summary>
        /// 소켓연결을 종료합니다.
        /// </summary>
        /// <param name="reuseSocket">소켓을 재사용할지 결정합니다.</param>
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
