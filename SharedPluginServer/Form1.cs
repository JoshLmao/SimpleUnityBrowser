using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MessageLibrary;
//using NamedPipeWrapper;
using SharedPluginServer.Interprocess;

namespace SharedPluginServer
{
    public partial class Form1 : Form
    {
        private static readonly log4net.ILog log =
    log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        Bitmap memoryImage;
       

        private SharedMemServer _memServer;

        private SocketServer _controlServer;

        private CefWorker _mainWorker;

        public Form1(CefWorker worker,SharedMemServer sharedMemServer,SocketServer csrv)
        {
            InitializeComponent();
            //_mainWorker = MainWorker;
            //_mainWorker = new CefWorker();
            //_mainWorker.Init();
            _mainWorker = worker;
            _memServer = sharedMemServer;

            _controlServer = csrv;

           //_controlServer.OnReceivedMessage += _memServer_OnMouseEvent;
        }

        private void _memServer_OnMouseEvent(MouseMessage msg)
        {
           // MessageBox.Show("_____MOUSE:" + msg.X + ";" + msg.Y);

            log.Info("________Mouse event:"+msg.X+";"+msg.Y+";"+msg.Type);
            switch (msg.Type)
            {
                case MouseEventType.LButtonDown:
                    _mainWorker.MouseEvent(msg.X, msg.Y, false);
                    break;
                case MouseEventType.LButtonUp:
                    _mainWorker.MouseEvent(msg.X, msg.Y, true);
                    break;
                case MouseEventType.Move:
                    _mainWorker.MouseMoveEvent(msg.X,msg.Y);
                    break;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
            //MainServer=new NamedPipeServer<string>("MainControlPipe");
          
           

            //MainServer.ClientMessage += MainServer_ClientMessage;
            //MainServer.ClientConnected += MainServer_ClientConnected;
            
           // MainServer.Start();
        }

        /*private void MainServer_ClientConnected(NamedPipeConnection<string, string> connection)
        {
            connection.PushMessage("Hello");
        }

        private void MainServer_ClientMessage(NamedPipeConnection<string, string> connection, string message)
        {
            MessageBox.Show(message);
        }*/

        private void button1_Click(object sender, EventArgs e)
        {
            /* using (Bitmap bmpScreenCapture = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                            Screen.PrimaryScreen.Bounds.Height))
             {
                 using (Graphics g = Graphics.FromImage(bmpScreenCapture))
                 {
                     g.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                      Screen.PrimaryScreen.Bounds.Y,
                                      0, 0,
                                      bmpScreenCapture.Size,
                                      CopyPixelOperation.SourceCopy);
                 }
                 pictureBox1.Image = bmpScreenCapture;
             }*/
            /* Graphics myGraphics = this.CreateGraphics();
             Size s = this.Size;
             memoryImage = new Bitmap(s.Width, s.Height, myGraphics);
             Graphics memoryGraphics = Graphics.FromImage(memoryImage);
             memoryGraphics.CopyFromScreen(this.Location.X, this.Location.Y, 0, 0, s);*/
            //memoryImage = ImageFromScreen();

            byte[] bytes= _mainWorker.GetBitmap();

            //IntPtr unmanagedPointer = Marshal.AllocHGlobal(bytes.Length);
           //Marshal.Copy(bytes, 0, unmanagedPointer, bytes.Length);
           //
            var bmp= new Bitmap(_mainWorker.GetWidth(), _mainWorker.GetHeight(), PixelFormat.Format32bppRgb);
            BitmapData bmd = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadWrite, bmp.PixelFormat);
            IntPtr pNative = bmd.Scan0;
            Marshal.Copy(bytes, 0, pNative, bytes.Length);
            bmp.UnlockBits(bmd);
            //  Marshal.FreeHGlobal(unmanagedPointer);



             pictureBox1.Image = bmp;

            // InitBuffer();
            _mainWorker.OnLoadFinished += _mainWorker_OnLoadFinished;
           _mainWorker.Navigate("http://www.google.com");
            //Thread controlThread = new Thread(ControlPipeServer);
            //controlThread.Start();
            System.Threading.Timer t = new System.Threading.Timer(RenderBitmap);
            t.Change(0, 1000);
        }

        public void RenderBitmap(Object stateInfo)
        {
            byte[] bytes = _mainWorker.GetBitmap();

            //IntPtr unmanagedPointer = Marshal.AllocHGlobal(bytes.Length);
            //Marshal.Copy(bytes, 0, unmanagedPointer, bytes.Length);
            //
            var bmp = new Bitmap(_mainWorker.GetWidth(), _mainWorker.GetHeight(), PixelFormat.Format32bppRgb);
            BitmapData bmd = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadWrite, bmp.PixelFormat);
            IntPtr pNative = bmd.Scan0;
            Marshal.Copy(bytes, 0, pNative, bytes.Length);
            bmp.UnlockBits(bmd);


            _memServer.WriteBytes(bytes);
            pictureBox1.Image = bmp;
        }

        private void _mainWorker_OnLoadFinished(int StatusCode)
        {
            byte[] bytes = _mainWorker.GetBitmap();

            //IntPtr unmanagedPointer = Marshal.AllocHGlobal(bytes.Length);
            //Marshal.Copy(bytes, 0, unmanagedPointer, bytes.Length);
            //
            var bmp = new Bitmap(_mainWorker.GetWidth(), _mainWorker.GetHeight(), PixelFormat.Format32bppRgb);
            BitmapData bmd = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadWrite, bmp.PixelFormat);
            IntPtr pNative = bmd.Scan0;
            Marshal.Copy(bytes, 0, pNative, bytes.Length);
            bmp.UnlockBits(bmd);

            pictureBox1.Image = bmp;
        }

        private static void ControlPipeServer(object data)
        {
            
        }

        public Bitmap ImageFromScreen()
        {
            Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            using (var gr = Graphics.FromImage(bmp))
            {
                gr.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y,
                    0, 0, Screen.PrimaryScreen.Bounds.Size);
            }
            return bmp;
        }

        void InitBuffer()
        {
            

            Rectangle rect=new Rectangle(0,0, memoryImage.Width, memoryImage.Height);
            BitmapData bmpData = memoryImage.LockBits(rect, ImageLockMode.ReadOnly, memoryImage.PixelFormat);

                int BufferSize = Math.Abs(bmpData.Stride) * memoryImage.Height;

            byte[] rgbVal = new byte[BufferSize];

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;
            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbVal, 0, BufferSize);

            memoryImage.UnlockBits(bmpData);

           // SharedBuf = new SharedMemory.SharedArray<byte>("MainBuffer", BufferSize);
            //SharedBuf.Write(rgbVal, 0);

            MessageBox.Show(BufferSize + ";" + memoryImage.PixelFormat+";"+ memoryImage.Width+";"+memoryImage.Height);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _mainWorker.Shutdown();
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            _mainWorker.MouseEvent(e.X, e.Y,false);
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            _mainWorker.MouseEvent(e.X, e.Y, true);
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            _mainWorker.MouseMoveEvent(e.X,e.Y);
        }
    }

  
}
