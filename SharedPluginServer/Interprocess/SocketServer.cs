using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MessageLibrary;



namespace SharedPluginServer.Interprocess
{
   public  class SocketServer
    {

        private static readonly log4net.ILog log =
 log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


       
        private  int myPort = 8885;
        private TcpListener _listener;

        private Thread listenerThread;

        public delegate void ReceivedMessage(EventPacket msg);

       private static volatile bool isWorking = true;

        public static event ReceivedMessage OnReceivedMessage;

        public void Init()
        {

            listenerThread = new Thread(new ThreadStart(ListenCallback));
            listenerThread.IsBackground = true;
            listenerThread.Start();
        }

       private void ListenCallback()
       {
            try
            {
                _listener = new TcpListener(System.Net.IPAddress.Any, myPort);
                _listener.Start();

                do
                {
                   
                   // UserConnection client = new UserConnection(_listener.AcceptTcpClient());
                   DoBeginAcceptTcpClient(_listener);
                   


                   // client.OnLineReceived += OnLineReceived;
                   
                } while (isWorking);

                _listener.Stop();
            }
            catch (Exception) //we have: A blocking operation was interrupted by a call to WSACancelBlockingCall
            {

                
            }
        }

        #region async stuff
        // Thread signal.
        public static ManualResetEvent tcpClientConnected =
    new ManualResetEvent(false);

        // Accept one client connection asynchronously.
        public static void DoBeginAcceptTcpClient(TcpListener
            listener)
        {
            // Set the event to nonsignaled state.
            tcpClientConnected.Reset();

            // Start to listen for connections from a client.
           // Console.WriteLine("Waiting for a connection...");

            // Accept the connection. 
            // BeginAcceptSocket() creates the accepted socket.
            listener.BeginAcceptTcpClient(
                new AsyncCallback(DoAcceptTcpClientCallback),
                listener);

            // Wait until a connection is made and processed before 
            // continuing.
            tcpClientConnected.WaitOne();
        }

        // Process the client connection.
        public static void DoAcceptTcpClientCallback(IAsyncResult ar)
        {
            if (isWorking)
            {
                // Get the listener that handles the client request.
                TcpListener listener = (TcpListener) ar.AsyncState;

                // End the operation and display the received data on 
                // the console.
                TcpClient client = listener.EndAcceptTcpClient(ar);

                UserConnection uclient = new UserConnection(client);

                uclient.OnLineReceived += OnLineReceived;
                // Signal the calling thread to continue.
                tcpClientConnected.Set();
            }

        }
        #endregion

        private static void OnLineReceived(UserConnection sender, byte[] data)
       {
            //send a message
           try
           {
                MemoryStream mstr = new MemoryStream(data);
                BinaryFormatter bf = new BinaryFormatter();
                EventPacket ep=bf.Deserialize(mstr) as EventPacket;
               if (ep != null)
               {
                   //log.Info("_________PACKET:"+ep.Type);
                   OnReceivedMessage?.Invoke(ep);
               }
            }
           catch (Exception)
           {
               
            //   throw;
           }
       }


       

       public void Shutdown()
       {
           isWorking = false;
           tcpClientConnected.Set();
           //_listener.Stop();
       }

       ~SocketServer()
       {
          Shutdown();  
       }
    }

#region client
    public delegate void LineReceive(UserConnection sender, byte[] Data);

    public class UserConnection
    {
        const int READ_BUFFER_SIZE = 2048;
        // Overload the new operator to set up a read thread.
        public UserConnection(TcpClient client)
        {
            this.client = client;
            // This starts the asynchronous read thread.  The data will be saved into
            // readBuffer.
            this.client.GetStream().BeginRead(readBuffer, 0, READ_BUFFER_SIZE, new AsyncCallback(StreamReceiver), null);
        }

        private TcpClient client;
        private byte[] readBuffer = new byte[READ_BUFFER_SIZE];

        public event LineReceive OnLineReceived;

        // This subroutine uses a StreamWriter to send a message to the user.
        public void SendData(byte[] Data)
        {
            //lock ensure that no other threads try to use the stream at the same time.
            lock (client.GetStream())
            {
                // StreamWriter writer = new StreamWriter(client.GetStream());
                //writer.Write(Data + (char)13 + (char)10);
                // Make sure all data is sent now.
                // writer.Flush();
                client.GetStream().Write(Data, 0, Data.Length);
            }
        }

        private void StreamReceiver(IAsyncResult ar)
        {
            int BytesRead;
           
            try
            {
                // Ensure that no other threads try to use the stream at the same time.
                lock (client.GetStream())
                {
                    // Finish asynchronous read into readBuffer and get number of bytes read.
                    BytesRead = client.GetStream().EndRead(ar);
                }
                // Convert the byte array the message was saved into, minus one for the
                // Chr(13).

                OnLineReceived?.Invoke(this, readBuffer);
                // Ensure that no other threads try to use the stream at the same time.
                lock (client.GetStream())
                {
                    // Start a new asynchronous read into readBuffer.
                    client.GetStream()
                        .BeginRead(readBuffer, 0, READ_BUFFER_SIZE, new AsyncCallback(StreamReceiver), null);
                }
            }
            catch (Exception e)
            {
            }
        }
    }
    #endregion
}
