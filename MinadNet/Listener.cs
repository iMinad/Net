using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MinadNet
{
    public abstract class Listener
    {
        //Args pool:
        private readonly BlockingCollection<AsyncAcceptArgs> AcceptArgs;

        //Socket:
        public readonly Socket Sock;
        public readonly int MaxAcceptOps;

        public Listener(Socket s, int maxAcceptOps=10)
        {
            AcceptArgs = new BlockingCollection<AsyncAcceptArgs>(maxAcceptOps);
            Sock = s;
            MaxAcceptOps = maxAcceptOps;
            for(int i = 0; i < maxAcceptOps; i++)
            {
                AsyncAcceptArgs args = AsyncAcceptArgs.Get(this);
                AcceptArgs.Add(args);
            }
        }

        public void StartAccepting(string address, int port)
        {
            Sock.Bind(new IPEndPoint(IPAddress.Parse(address), port));
            Sock.Listen(MaxAcceptOps);

            while (true)
            {
                AsyncAcceptArgs args = AcceptArgs.Take();
                args.AcceptSocket = null;
                accept(args);
            }
        }

        private void accept(SocketAsyncEventArgs args)
        {
            if (!Sock.AcceptAsync(args))
                on_accept(null, args);
        }

        internal void on_accept(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success) OnConnection(args.AcceptSocket);
            else OnSocketError(args.SocketError);

            AcceptArgs.Add((AsyncAcceptArgs)args);
        }

        public abstract void OnConnection(Socket s);
        public virtual void OnSocketError(SocketError err) { }
    }
}
