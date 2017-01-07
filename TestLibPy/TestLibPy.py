import clr
clr.AddReference('MinadNet')
from System import AppDomain
AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", "./app.config")


import time
import gc
from System import Console
from System.Net.Sockets import *
from System.Net import *
from MinadNet import *
from MinadNet.Pools import BufferPool

gc.disable()
MainBufferPool = BufferPool(1024)


class EchoServer(Listener):
    def OnConnection(self, sock):
        #print("New connection from %s"%sock.RemoteEndPoint.ToString())
        newClient = EchoClient(sock, False)
        newClient.setReadBuf()
        newClient.setName("CLIENT-SERVER")
        newClient.Receive(newClient.ReadBuf)

class EchoClient(Client):
    def setName(self, name):
        self.name = name

    def setReadBuf(self):
        self.ReadBuf = MainBufferPool.Get()
        self.ReadBuf.Ref()

    def OnConnect(self):
        print("Connected!")
        self.setReadBuf()
        self.Send(ByteBuffer("<policy-file-request>\x00"))
        self.Receive(self.ReadBuf) #Receive into readbuf

    def OnData(self, buf, bytesTransfered):
        self.ReadBuf.Set(self.ReadBuf.Offset, bytesTransfered)
        self.Send(self.ReadBuf) #Send read buffer

    def OnSend(self, buf):
        self.ReadBuf.Reset()
        self.Receive(self.ReadBuf) #Receive into read buf

    def OnError(self, ex):
        print(ex)
        self.Close();

    def OnSocketError(self, args, err):
        print(err)
        self.Close();

    def OnClose(self):
        self.ReadBuf.DeRef()
        print("Closed!")

try:
    client = EchoClient(Socket(SocketType.Stream, ProtocolType.Tcp))
    client.setName("CLIENT")
    #client.Connect("127.0.0.1", 5588)

    server = EchoServer(Socket(SocketType.Stream, ProtocolType.Tcp))
    server.StartAccepting("127.0.0.1", 5588)
except Exception as ex:
    print(ex)