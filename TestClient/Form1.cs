#define USE_ARGS

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using SharedMemory;
using System.IO.Pipes;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using SharedMemory;
using log4net;
using MessageLibrary;

namespace TestClient
{
    public partial class Form1 : Form
    {
        private static readonly log4net.ILog log =
  log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);



        const int READ_BUFFER_SIZE = 2048;
        private byte[] readBuffer = new byte[READ_BUFFER_SIZE];
        //const int BMWidth = 1280;
        //const int BMHeight = 1024;


        private int posX = 0;
        private int posY = 0;

        private TcpClient clientSocket;

        //  private Queue<MouseMessage> _sendEvents; 

        private SharedArray<byte> arr; //= new SharedArray<byte>("MainSharedMem");

        private Bitmap _texture;

        public string memfile= "MainSharedMem";

        public int port = 8885;

        private bool _inModalDialog = false;

        public Form1()
        {
            InitializeComponent();
            this.pictureBox1.MouseWheel += PictureBox1_MouseWheel;

           Init();
        }


        public void Init()
        {


            //string args = "--enable-media-stream";
            string args = "";
#if USE_ARGS
           


            args =args+ pictureBox1.Width.ToString() + " " + pictureBox1.Height.ToString()+" ";
            args = args + "http://test.webrtc.org"+" ";
            Guid memid = Guid.NewGuid();

            memfile = memid.ToString();
            args = args + memfile + " ";
          Random r=new Random();
            port = 8880 + r.Next(10);

         
            args = args + port.ToString()+" ";
            args = args + "1";//webrtc
#endif


            Process pluginProcess = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    WorkingDirectory =
                        //  @"D:\work\unity\StandaloneConnector\SharedPluginServerClean\UnityClient\Output\x86\PluginServer",
                        @"D:\work\unity\StandaloneConnector_1\SimpleUnityBrowser\SharedPluginServer\bin\x64\Debug",
                    FileName =
                     //@"D:\work\unity\StandaloneConnector\SharedPluginServerClean\UnityClient\Output\x86\PluginServer\SharedPluginServer.exe",
                        @"D:\work\unity\StandaloneConnector_1\SimpleUnityBrowser\SharedPluginServer\bin\x64\Debug\SharedPluginServer.exe",
                    Arguments = args
                    
                }
            };
            pluginProcess.Start();
            //Thread.Sleep(10000);
            // while (!Process.GetProcesses().Any(p => p.Name == myName)) { Thread.Sleep(100); }

            bool found_proc = false;
            while (!found_proc)
            {
                foreach (Process clsProcess in Process.GetProcesses())
                    if (clsProcess.ProcessName == pluginProcess.ProcessName)
                        found_proc = true;

                Thread.Sleep(1000);
            }

            arr = new SharedArray<byte>(memfile);

            //Connect
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            //clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket=new TcpClient("127.0.0.1", port);

            //clientSocket.Connect(new IPEndPoint(ip, port));

            //Receive
            this.clientSocket.GetStream().BeginRead(readBuffer, 0, READ_BUFFER_SIZE, new AsyncCallback(StreamReceiver), null);
#if USE_ARGS
            _texture = new Bitmap(pictureBox1.Width, pictureBox1.Width);
#else
            int defWidth = 1280;
            int defHeight = 720;
            _texture = new Bitmap(defWidth, defHeight);
#endif
            Application.Idle += Application_Idle;
        }


        private void StreamReceiver(IAsyncResult ar)
        {
            int BytesRead;

            try
            {
                // Ensure that no other threads try to use the stream at the same time.
                lock (clientSocket.GetStream())
                {
                    // Finish asynchronous read into readBuffer and get number of bytes read.
                    BytesRead = clientSocket.GetStream().EndRead(ar);
                }
               
                MemoryStream mstr = new MemoryStream(readBuffer);
                BinaryFormatter bf = new BinaryFormatter();
                EventPacket ep = bf.Deserialize(mstr) as EventPacket;
                if (ep != null)
                {
                   // if (ep.Type == BrowserEventType.Ping)
                    //    SendPing();
                    //  //  MessageBox.Show("PINGBACK");

                    if (ep.Type == BrowserEventType.Dialog)
                    {
                        DialogEvent dev=ep.Event as DialogEvent;
                        switch (dev.Type)
                        {
                                case DialogEventType.Alert:
                            {
                                _inModalDialog = true;
                                MessageBox.Show(dev.Message, "InApp");
                                    SendDialogResponse(true);
                                
                                    _inModalDialog = false;
                                    break;
                            }

                            case DialogEventType.Confirm:
                            {
                                    _inModalDialog = true;
                                if (MessageBox.Show(dev.Message, "InApp", MessageBoxButtons.OKCancel) == DialogResult.OK)
                                {
                                        SendDialogResponse(true);
                                }
                                else
                                {
                                        SendDialogResponse(false);
                                 }
                                    _inModalDialog = false;
                                    break;
                            }
                        }
                    }

                    if (ep.Type == BrowserEventType.Generic)
                    {
                        GenericEvent ge=ep.Event as GenericEvent;

                        if (ge.Type == GenericEventType.JSQuery)
                        {
                            if(MessageBox.Show("JS QUERY:" + ge.JsQuery,"Query",MessageBoxButtons.OKCancel)==DialogResult.OK)
                                SendQueryResponse("Query ok");
                            else
                                SendQueryResponse("Query cancel");
                        }

                        if (ge.Type == GenericEventType.PageLoaded)
                        {
                            MessageBox.Show("Navigated to:" + ge.NavigateUrl);
                        }
                    }
                }

                // Ensure that no other threads try to use the stream at the same time.
                   lock (clientSocket.GetStream())
                {
                    // Start a new asynchronous read into readBuffer.
                    clientSocket.GetStream()
                        .BeginRead(readBuffer, 0, READ_BUFFER_SIZE, new AsyncCallback(StreamReceiver), null);
                }
            }
            catch (Exception e)
            {
            }
        }

        public void SendDialogResponse(bool ok)
        {
            DialogEvent de = new DialogEvent()
            {
                GenericType = BrowserEventType.Dialog,
                success = ok,
                input = ""
            };

            EventPacket ep = new EventPacket
            {
                Event = de,
                Type = BrowserEventType.Dialog
            };

            MemoryStream mstr = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(mstr, ep);
            byte[] b = mstr.GetBuffer();

            //
            lock (clientSocket.GetStream())
            {
                clientSocket.GetStream().Write(b, 0, b.Length);
            }

        }

        public void SendQueryResponse(string response)
        {
            GenericEvent ge = new GenericEvent()
            {
                Type = GenericEventType.JSQueryResponse,
                GenericType = BrowserEventType.Generic,
                JsQueryResponse = response
            };

            EventPacket ep = new EventPacket()
            {
                Event = ge,
                Type = BrowserEventType.Generic
            };

            MemoryStream mstr = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(mstr, ep);
            byte[] b = mstr.GetBuffer();
            //
            lock (clientSocket.GetStream())
            {
                clientSocket.GetStream().Write(b, 0, b.Length);
            }
        }


        private void Application_Idle(object sender, EventArgs e)
        {
            //Render bitmap
           // MessageBox.Show("RENDER");
            byte[] _read = new byte[arr.Length];

            arr.CopyTo(_read, 0);

            
            Rectangle rect = new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height);
            BitmapData bmpData = _texture.LockBits(rect, ImageLockMode.WriteOnly, _texture.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(_read, 0, ptr, _read.Length);
            _texture.UnlockBits(bmpData);
            pictureBox1.Image = _texture;

            //Query message
            //clientSocket.Receive()

        }

        

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SendShutdownEvent();
            Application.Idle -= Application_Idle;
            clientSocket.Close();
            
        }

        public void SendMouseEvent(MouseMessage msg)
        {
           // if (_MouseDone)
                //_controlMem.Write(ref msg,0);
           // _MouseDone = false;

            if (!_inModalDialog)
            {
                EventPacket ep = new EventPacket
                {
                    Event = msg,
                    Type = BrowserEventType.Mouse
                };

                MemoryStream mstr = new MemoryStream();
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(mstr, ep);
                byte[] b = mstr.GetBuffer();

                //
                lock (clientSocket.GetStream())
                {
                    clientSocket.GetStream().Write(b, 0, b.Length);
                }
            }
            //  MessageBox.Show(_sendEvents.Count.ToString());
        }

        public void SendShutdownEvent()
        {
            GenericEvent ge = new GenericEvent()
            {
                Type = GenericEventType.Shutdown,
                 GenericType = BrowserEventType.Generic
            };

            EventPacket ep = new EventPacket()
            {
                Event = ge,
                Type = BrowserEventType.Generic
            };

            MemoryStream mstr = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(mstr, ep);
            byte[] b = mstr.GetBuffer();
            //
            lock (clientSocket.GetStream())
            {
                clientSocket.GetStream().Write(b, 0, b.Length);
            }
            //  MessageBox.Show(_sendEvents.Count.ToString());

        }

        public void SendPing()
        {
            GenericEvent ge = new GenericEvent()
            {
                Type = GenericEventType.Navigate, //could be any
                GenericType = BrowserEventType.Ping,
                
            };

            EventPacket ep = new EventPacket()
            {
                Event = ge,
                Type = BrowserEventType.Ping
            };

            MemoryStream mstr = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(mstr, ep);
            byte[] b = mstr.GetBuffer();
            //
            lock (clientSocket.GetStream())
            {
                clientSocket.GetStream().Write(b, 0, b.Length);
            }
        }

        public void SendNavigateEvent(string url,bool back,bool forward)
        {
            GenericEvent ge = new GenericEvent()
            {
                Type = GenericEventType.Navigate,
                GenericType = BrowserEventType.Generic,
                NavigateUrl = url
            };

            if(back)
                ge.Type=GenericEventType.GoBack;
            else if(forward)
                ge.Type=GenericEventType.GoForward;

            EventPacket ep = new EventPacket()
            {
                Event = ge,
                Type = BrowserEventType.Generic
            };

            MemoryStream mstr = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(mstr, ep);
            byte[] b = mstr.GetBuffer();
            //
            lock (clientSocket.GetStream())
            {
                clientSocket.GetStream().Write(b, 0, b.Length);
            }
            //  MessageBox.Show(_sendEvents.Count.ToString());
        }

        public void SendExecuteJSEvent(string js)
        {
            GenericEvent ge = new GenericEvent()
            {
                Type = GenericEventType.ExecuteJS,
                GenericType = BrowserEventType.Generic,
                JsCode= js
            };

            EventPacket ep = new EventPacket()
            {
                Event = ge,
                Type = BrowserEventType.Generic
            };

            MemoryStream mstr = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(mstr, ep);
            byte[] b = mstr.GetBuffer();
            //
            lock (clientSocket.GetStream())
            {
                clientSocket.GetStream().Write(b, 0, b.Length);
            }
            //  MessageBox.Show(_sendEvents.Count.ToString());
        }

        public void SendCharEvent(int character,KeyboardEventType type)
        {
            log.Info("____________CHAR EVENT:"+character);
            KeyboardEvent keyboardEvent = new KeyboardEvent()
            {
                Type = type,
                Key = character
            };
            EventPacket ep = new EventPacket()
            {
                Event = keyboardEvent,
                Type = BrowserEventType.Keyboard
            };

            MemoryStream mstr = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(mstr, ep);
            byte[] b = mstr.GetBuffer();
            //
            lock (clientSocket.GetStream())
            {
                clientSocket.GetStream().Write(b, 0, b.Length);
            }
            //  MessageBox.Show(_sendEvents.Count.ToString());
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            MouseMessage msg = new MouseMessage
            {
                Type=MouseEventType.ButtonDown,
                X=e.X,
                Y=e.Y,
                GenericType = BrowserEventType.Mouse
            };

            SendMouseEvent(msg);
        }

        private void PictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            //pictureBox1.Focus();
            MouseMessage msg = new MouseMessage
            {
                Type = MouseEventType.Wheel,
                X = e.X,
                Y = e.Y,
                Delta = e.Delta,
                GenericType = BrowserEventType.Mouse,
                Button = MouseButton.None
            };

            if(e.Button==MouseButtons.Left)
                msg.Button=MouseButton.Left;
            if (e.Button == MouseButtons.Right)
                msg.Button = MouseButton.Right;
            if (e.Button == MouseButtons.Middle)
                msg.Button = MouseButton.Middle;

            SendMouseEvent(msg);
        }

        //really spammy
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
           // MessageBox.Show("_______MOVE 1");
            if (posX != e.X || posY != e.Y)
            {
                MouseMessage msg = new MouseMessage
                {
                    Type = MouseEventType.Move,
                    X = e.X,
                    Y = e.Y,
                    GenericType = BrowserEventType.Mouse,
                     Delta = e.Delta,
                    Button = MouseButton.None
                };

                if (e.Button == MouseButtons.Left)
                    msg.Button = MouseButton.Left;
                if (e.Button == MouseButtons.Right)
                    msg.Button = MouseButton.Right;
                if (e.Button == MouseButtons.Middle)
                    msg.Button = MouseButton.Middle;
                posX = e.X;
                posY = e.Y;

                SendMouseEvent(msg);
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            MouseMessage msg = new MouseMessage
            {
                Type = MouseEventType.ButtonUp,
                X = e.X,
                Y = e.Y,
                GenericType = BrowserEventType.Mouse,
                Button = MouseButton.None
            };

            if (e.Button == MouseButtons.Left)
                msg.Button = MouseButton.Left;
            if (e.Button == MouseButtons.Right)
                msg.Button = MouseButton.Right;
            if (e.Button == MouseButtons.Middle)
                msg.Button = MouseButton.Middle;


            SendMouseEvent(msg);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
          
        }

        

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            SendCharEvent((int)e.KeyValue, KeyboardEventType.Down);
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            SendCharEvent((int)e.KeyChar, KeyboardEventType.CharKey);
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            SendCharEvent((int)e.KeyValue, KeyboardEventType.Up);
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            MouseMessage msg = new MouseMessage
            {
                Type = MouseEventType.Leave,
                X = 0,
                Y = 0,
                GenericType = BrowserEventType.Mouse
            };

            SendMouseEvent(msg);
        }

        private void pictureBox1_MouseHover(object sender, EventArgs e)
        {
            pictureBox1.Focus();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
           // MessageBox.Show(textBox1.Text);
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendNavigateEvent(textBox1.Text,false,false);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            SendExecuteJSEvent("alert('Hello world');");
          //SendPing();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SendNavigateEvent("", true, false);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SendNavigateEvent("", false, true);
        }

        //protected override void OnMouseWheel()
    }






   
}
