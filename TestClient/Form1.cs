using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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


       

        const int BMWidth = 1280;
        const int BMHeight = 1024;

        private bool _MouseDone = true;

        private int posX = 0;
        private int posY = 0;

        private Socket clientSocket;

      //  private Queue<MouseMessage> _sendEvents; 

        public Form1()
        {
            InitializeComponent();


            //Connect
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(new IPEndPoint(ip, 8885));

            /// _sendEvents=new Queue<MouseMessage>();

        }

        

        private void button1_Click(object sender, EventArgs e)
        {
            SharedArray<byte> arr = new SharedArray<byte>("MainSharedMem");

            byte[] _read = new byte[arr.Length];

            arr.CopyTo(_read, 0);

            Bitmap bmp = new Bitmap(BMWidth, BMHeight);
            Rectangle rect = new Rectangle(0, 0, BMWidth, BMHeight);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(_read, 0, ptr, _read.Length);
            bmp.UnlockBits(bmpData);

            pictureBox1.Image = bmp;

            ////




          //  using (StreamWriter sw = new StreamWriter(pipeClient))
           // {
           //     sw.WriteLine("TEST PIPE SEND");
           // }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            clientSocket.Close();
        }

        public void SendMouseEvent(MouseMessage msg)
        {
           // if (_MouseDone)
                //_controlMem.Write(ref msg,0);
           // _MouseDone = false;
           MemoryStream mstr=new MemoryStream();
            BinaryFormatter bf=new BinaryFormatter();
            bf.Serialize(mstr, msg);
            byte[] b = mstr.GetBuffer();
            clientSocket.Send(b);
            //  MessageBox.Show(_sendEvents.Count.ToString());
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            MouseMessage msg = new MouseMessage
            {
                Type=MouseEventType.LButtonDown,
                X=e.X,
                Y=e.Y
            };

            SendMouseEvent(msg);
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
           // MessageBox.Show("_______MOVE 1");
            //if (posX != e.X || posY != e.Y)
           // {
                MouseMessage msg = new MouseMessage
                {
                    Type = MouseEventType.Move,
                    X = e.X,
                    Y = e.Y
                };
                posX = e.X;
                posY = e.Y;

                SendMouseEvent(msg);
            //}
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            MouseMessage msg = new MouseMessage
            {
                Type = MouseEventType.LButtonUp,
                X = e.X,
                Y = e.Y
            };

            SendMouseEvent(msg);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
          
        }
    }



    


    #region Utils
    public class StreamString
    {
        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            int len = 0;

            len = ioStream.ReadByte() * 256;
            len += ioStream.ReadByte();
            byte[] inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);

            return streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }
            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }

    // Contains the method executed in the context of the impersonated user
    public class ReadFileToStream
    {
        private string fn;
        private StreamString ss;

        public ReadFileToStream(StreamString str, string filename)
        {
            fn = filename;
            ss = str;
        }

        public void Start()
        {
            string contents = File.ReadAllText(fn);
            ss.WriteString(contents);
        }
    }
    #endregion
}
