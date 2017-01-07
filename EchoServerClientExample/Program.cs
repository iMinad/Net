using System.Net.Sockets;

namespace EchoServerClientExample
{
    class Program
    {
        static void Main(string[] args)
        {
            EchoClient client = new EchoClient(new Socket(SocketType.Stream, ProtocolType.Tcp));
            client.Connect("127.0.0.1", 5588);

            EchoListener listener = new EchoListener(new Socket(SocketType.Stream, ProtocolType.Tcp));
            listener.StartAccepting("127.0.0.1", 5588);
        }
    }
}
