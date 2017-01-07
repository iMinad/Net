using System;
using System.Net.Sockets;
using MinadNet;

namespace EchoServerClientExample
{
    class EchoListener : Listener
    {
        public EchoListener(Socket s, int maxAcceptOps = 10) : base(s, maxAcceptOps) {}

        public override void OnConnection(Socket s)
        {
            EchoClientSimple client = new EchoClientSimple(s);
            client.Receive(client.ReadBuffer); //Start recv into its read buffer.
        }
    }
}
