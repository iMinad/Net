using System;
using System.Net.Sockets;
using System.Collections.Concurrent;

namespace MinadNet
{
    internal delegate void IO_Completed(object sender, SocketAsyncEventArgs args);

    public enum ArgsType
    {
        Read,
        Send,
        Connect,
        Accept
    }

    public abstract class AsyncArgs : SocketAsyncEventArgs
    {
        public abstract ArgsType Type();
        protected abstract void on_io(object sender, SocketAsyncEventArgs args);
    }

    public sealed class AsyncReadArgs : AsyncArgs
    {
        private static readonly ConcurrentBag<AsyncReadArgs> AsyncReadArgsPool = new ConcurrentBag<AsyncReadArgs>();
        private volatile Client client;

        public static AsyncReadArgs Get(Client c)
        {
            AsyncReadArgs args = null;
            AsyncReadArgsPool.TryTake(out args);
            if (args == null) args = new AsyncReadArgs();
            args.client = c;

            return args;
        }

        public static void Return(AsyncReadArgs args)
        {
            if (args != null) AsyncReadArgsPool.Add(args);
        }


        private AsyncReadArgs()
        {
            base.Completed += new EventHandler<SocketAsyncEventArgs>(on_io);
        }

        public override ArgsType Type()
        {
            return ArgsType.Read;
        }

        protected override void on_io(object sender, SocketAsyncEventArgs args)
        {
            try
            {
                client.read_complete(sender, args);
            }
            catch (Exception ex)
            {
                client.OnError(ex);
            }
        }
    }

    public sealed class AsyncSendArgs : AsyncArgs
    {
        private static readonly ConcurrentBag<AsyncSendArgs> AsyncSendArgsPool = new ConcurrentBag<AsyncSendArgs>();
        private volatile Client client;

        public static AsyncSendArgs Get(Client c)
        {
            AsyncSendArgs args = null;
            AsyncSendArgsPool.TryTake(out args);
            if (args == null) args = new AsyncSendArgs();
            args.client = c;

            return args;
        }

        public static void Return(AsyncSendArgs args)
        {
            if (args != null) AsyncSendArgsPool.Add(args);
        }

        private AsyncSendArgs()
        {
            base.Completed += new EventHandler<SocketAsyncEventArgs>(on_io);
        }

        public override ArgsType Type()
        {
            return ArgsType.Send;
        }

        protected override void on_io(object sender, SocketAsyncEventArgs args)
        {
            try
            {
                client.send_complete(sender, args);
            }
            catch (Exception ex)
            {
                client.OnError(ex);
            }
        }
    }


    public sealed class AsyncAcceptArgs : AsyncArgs
    {
        private static readonly ConcurrentStack<AsyncAcceptArgs> AsyncAcceptArgsPool = new ConcurrentStack<AsyncAcceptArgs>();
        private volatile Listener listener;

        public static AsyncAcceptArgs Get(Listener l)
        {
            AsyncAcceptArgs args = null;
            AsyncAcceptArgsPool.TryPop(out args);
            if (args == null) args = new AsyncAcceptArgs();
            args.listener = l;

            return args;
        }

        public static void Return(AsyncAcceptArgs args)
        {
            if (args != null) AsyncAcceptArgsPool.Push(args);
        }

        private AsyncAcceptArgs()
        {
            base.Completed += new EventHandler<SocketAsyncEventArgs>(on_io);
        }

        public override ArgsType Type()
        {
            return ArgsType.Accept;
        }

        protected override void on_io(object sender, SocketAsyncEventArgs args)
        {
            listener.on_accept(sender, args);
        }
    }

    public sealed class AsyncConnectArgs : AsyncArgs
    {
        internal AsyncConnectArgs(IO_Completed fn)
        {
            base.Completed += new EventHandler<SocketAsyncEventArgs>(fn);
        }

        public override ArgsType Type()
        {
            return ArgsType.Connect;
        }

        protected override void on_io(object sender, SocketAsyncEventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
