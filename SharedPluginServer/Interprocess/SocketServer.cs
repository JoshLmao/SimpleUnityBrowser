using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;

using System.Net.Sockets;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;
using MessageLibrary;


namespace SharedPluginServer.Interprocess
{
   public  class SocketServer
    {

        private static readonly log4net.ILog log =
 log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        private static byte[] result = new byte[2048];
        private  int myPort = 8885;
        Socket _serverSocket;

        private Socket _clientSocket=null;

       private static volatile bool _shouldStop = false;

        public delegate void ReceivedMessage(EventPacket msg);

       private Thread _connectThread;
       private Thread _listenThread;

        public event ReceivedMessage OnReceivedMessage;

        public void Init()
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(new IPEndPoint(ip, myPort));
            _serverSocket.Listen(10);
            log.Info("_____LISTEN___");
            _connectThread = new Thread(ListenClientConnect);
            _connectThread.Start(this);
        }

        private  static void ListenClientConnect(object srv)
        {
            SocketServer _server=srv as SocketServer;

            while (!_shouldStop)
            {
                
                _server._clientSocket = _server._serverSocket.Accept();
                // if (!m_ClientSocketList.Contains(socket))
                //     m_ClientSocketList.Add(socket);
                //  socket.Send(Encoding.UTF8.GetBytes("Connect to server!!!"));
                log.Info("_____CONNECTED___");
                _server._listenThread = new Thread(_server.ReceiveMessage);
                _server._listenThread.Start(_server);
            }
        }

       // void SendString(string str)
        //{
         //   _clientSocket.Send
       // }

        private  void ReceiveMessage(object srv)
        {
            SocketServer _server = (SocketServer)srv;
            while (!_shouldStop)
            {
                try
                {
                    
                    int receiveNumber = _server._clientSocket.Receive(result);

                   // log.Info("__RECEIVED___:"+receiveNumber);
                    try
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        MemoryStream str = new MemoryStream(result);
                        EventPacket msg = (EventPacket)bf.Deserialize(str);

                        _server.OnReceivedMessage?.Invoke(msg);
                    }
                    catch (Exception ex)
                    {
                        log.Error("_______SERIALIZATION ERROR:"+ex.Message);
                    }
//string data = Encoding.ASCII.GetString(result, 0, receiveNumber);
                    
                   
                    //call something
                }
                catch (Exception ex)
                {
                   log.Error("_______SOCKET:"+ex.Message);
                    //m_ClientSocketList.Remove(myClientSocket);
                    if (_server != null)
                    {
                        _server._clientSocket?.Shutdown(SocketShutdown.Both);
                        _server._clientSocket?.Close();
                    }
                    break;
                }
            }
        }

       public void Shutdown()
       {
            _clientSocket?.Shutdown(SocketShutdown.Both);
            _clientSocket?.Close();

           // MessageBox.Show("_______SHUTDOWN");
           _shouldStop = true;
            //_connectThread?.Abort();
            //_listenThread?.Abort();
       }

       ~SocketServer()
       {
          Shutdown();  
       }
    }



   
}
