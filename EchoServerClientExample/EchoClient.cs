

using System;
using System.Net.Sockets;
using MinadNet;
using MinadNet.Pools;
using System.Threading.Tasks;

namespace EchoServerClientExample
{
    static class Packets
    {
        public static readonly ByteBuffer FirstPacket = new ByteBuffer("Hello!");
    }

    public class EchoClient : Client
    {
        private static readonly BufferPool buffers = new BufferPool(1024);
        public ByteBuffer ReadBuffer = buffers.Get();
        
        public EchoClient(Socket s) : base(s, true)
        {
            ReadBuffer.Ref(); //Add ref to buffer.
        }

        public override void OnConnect()
        {
            this.Send(Packets.FirstPacket);
            this.Receive(ReadBuffer); //Start receiving into read buffer. OnData will be called when data arrives!
        }

        //Async for delay.
        public override async void OnData(ByteBuffer buf, int bytesTransfered)
        {
            string packet = buf.ToString(buf.Offset, bytesTransfered);
            Console.WriteLine("[CLIENT]Recv: " + packet);
            await Task.Delay(1000); //Wait 1 second.
            this.Send(new ByteBuffer(packet)); //Send packet.
            this.Receive(ReadBuffer); //Receive into read buffer.
        }

        public override void OnSend(ByteBuffer buf)
        {
            //Do nothing, if you have booled send buffer
            //You could return it to pool
            //or keep sending it.
            Console.WriteLine("[CLIENT]Sent: " + buf.ToString(buf.Offset, SendArgs.BytesTransferred));
        }

        public override void OnClose()
        {
            ReadBuffer.DeRef();
        }

        public override void OnError(Exception ex)
        {
            Console.WriteLine("[CLIENT]Error " + ex);
        }

        //AsyncArgs is SocketAsyncEventArgs
        public override void OnSocketError(AsyncArgs args, SocketError err)
        {
            Console.WriteLine("[CLIENT]SocketError " + err);
        }
    }
}
