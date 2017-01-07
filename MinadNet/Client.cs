using System.Net.Sockets;
using System;
using System.Net;

namespace MinadNet
{
    public abstract class Client
    {
        //All args:
        protected readonly AsyncReadArgs ReadArgs;
        protected readonly AsyncSendArgs SendArgs;
        protected readonly AsyncConnectArgs ConnectArgs;

        //States:
        private bool stateConnected;
        private bool stateConnecting;
        private bool stateReceiving;
        private bool stateSending;
        private bool stateClosed;
        
        //Socket:
        public readonly Socket Sock;

        //CurrentMessage, Messages and mutex:
        private ByteBuffer SendBuf;
        private readonly BufferQueue Buffers = new BufferQueue();
        private readonly object m = new object();

        public Client(Socket s, bool needsConnect=true)
        {
            Sock = s;

            ReadArgs = AsyncReadArgs.Get(this);
            SendArgs = AsyncSendArgs.Get(this);
            if (needsConnect) ConnectArgs = new AsyncConnectArgs(connect_complete);
            else stateConnected = true;
        }

        //Callbacks:
        #region CallBacks
        internal void read_complete(object sender, SocketAsyncEventArgs args)
        {
            lock (m) { stateReceiving = false; };

            switch (ReadArgs.SocketError)
            {
                case SocketError.Success:
                    if (ReadArgs.BytesTransferred > 0)
                        OnData((ByteBuffer)ReadArgs.UserToken, ReadArgs.BytesTransferred);
                    else
                        OnSocketError(ReadArgs, SocketError.NoData);
                    break;

                default:
                    OnSocketError(ReadArgs, ReadArgs.SocketError);
                    break;
            }
        }

        internal void send_complete(object sender, SocketAsyncEventArgs args)
        {
            switch (SendArgs.SocketError)
            {
                case SocketError.Success:
                    OnSend(SendBuf);
                    break;

                default:
                    OnSocketError(SendArgs, SendArgs.SocketError);
                    break;
            }

            lock (m)
            {
                if (stateClosed) return;
                ByteBuffer buf = this.Buffers.TryDequeue();
                if (buf == null) { stateSending = false; return; }
                SendBuf = buf;
                SendArgs.SetBuffer(buf.Buf, buf.Offset, buf.Length);
                if (!Sock.SendAsync(SendArgs)) send_complete(null, SendArgs);
            }
        }

        internal void connect_complete(object sender, SocketAsyncEventArgs args)
        {
            lock (m) { stateConnecting = false; stateConnected = true; };

            switch (args.SocketError)
            {
                case SocketError.Success:
                    OnConnect();
                    break;

                default:
                    OnSocketError((AsyncArgs)args, args.SocketError);
                    break;
            }
        }
        #endregion

        #region SealedMethods
        public void Receive(ByteBuffer buffer)
        {
            lock (m) { if (!stateConnected || stateReceiving || stateClosed) return; stateReceiving = true; }
            ReadArgs.UserToken = buffer;
            ReadArgs.SetBuffer(buffer.Buf, buffer.Offset, buffer.Length);

            try
            {
                if (!Sock.ReceiveAsync(ReadArgs)) read_complete(null, ReadArgs);
            }
            catch(Exception ex)
            {
                OnError(ex);
            }
        }

        public void Send(ByteBuffer buf)
        {
            try
            {
                lock (m)
                {
                    if (!stateConnected || stateClosed) return;

                    if (!stateSending)
                    {
                        stateSending = true;
                        SendBuf = buf;
                        SendArgs.SetBuffer(buf.Buf, buf.Offset, buf.Length);
                        if (!Sock.SendAsync(SendArgs)) send_complete(null, SendArgs);
                    }
                    else Buffers.Enqueue(buf);
                }
            }
            catch(Exception ex)
            {
                OnError(ex);
            }
        }

        public void Connect(string address, int port)
        {
            lock (m) { if (stateConnected || stateConnecting || stateClosed) return; stateConnecting = true;
                ConnectArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Parse(address), port);
            };

            try
            {
                if (!Sock.ConnectAsync(ConnectArgs)) connect_complete(null, ConnectArgs);
            }
            catch(Exception ex)
            {
                OnError(ex);
            }
        }

        public void Close()
        {
            lock(m){ if (stateClosed) return; stateClosed = true; }

            try
            {
                Sock.Close();
            } finally
            {
                lock(m)
                {
                    ByteBuffer buf;
                    while((buf = Buffers.TryDequeue()) != null)
                    {
                        buf.DeRef();
                    }

                    AsyncReadArgs.Return(ReadArgs);
                    AsyncSendArgs.Return(SendArgs);
                }

                OnClose();
            }
        }
        #endregion

        //Abstract methods:
        #region AbstractMethods
        public abstract void OnSocketError(AsyncArgs args, SocketError err);
        public abstract void OnError(Exception ex);
        public abstract void OnData(ByteBuffer buf, int bytesTransfered);
        public abstract void OnSend(ByteBuffer buf);
        public abstract void OnConnect();
        public abstract void OnClose();
        #endregion
    }
}
