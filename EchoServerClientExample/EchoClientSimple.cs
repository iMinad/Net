using System;
using System.Net.Sockets;
using MinadNet;
using MinadNet.Pools;

namespace EchoServerClientExample
{
    public class EchoClientSimple : Client
    {
        private static readonly BufferPool buffers = new BufferPool(1024);
        public ByteBuffer ReadBuffer = buffers.Get(); //Read into this buf, then send it OnData.

        public EchoClientSimple(Socket s) : base(s, false)
        {
            ReadBuffer.Ref(); //Add ref counter.
        }

        public override void OnConnect() {} //Do nothing this class is meant for server.

        public override void OnData(ByteBuffer buf, int bytesTransfered)
        {
            buf.Set(buf.Offset, bytesTransfered);
            this.Send(buf);
        }

        public override void OnSend(ByteBuffer buf)
        {
            this.Receive(buf); //Receive into buf, which is ReadBuffer.
        }

        public override void OnClose()
        {
            ReadBuffer.DeRef(); //De ref, so buffer can be returned to pool.
        }

        public override void OnError(Exception ex)
        {
            Console.WriteLine("Error: " + ex.ToString());
            Close();
        }

        public override void OnSocketError(AsyncArgs args, SocketError err)
        {
            Console.WriteLine("Socket error: " + err.ToString());
            Close();
        }
    }
}
