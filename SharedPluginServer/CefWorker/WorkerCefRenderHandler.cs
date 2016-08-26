using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Xilium.CefGlue;

namespace SharedPluginServer
{
    class WorkerCefRenderHandler : CefRenderHandler
    {
        private readonly int _windowHeight;
        private readonly int _windowWidth;

        private int _copysize = 0;

        public byte[] MainBitmap = null;

        public int CurrentWidth=0;
        public int CurrentHeight = 0;

        public SharedMemServer _memServer = null;

        public WorkerCefRenderHandler(int windowWidth, int windowHeight)
        {
            _windowWidth = windowWidth;
            _windowHeight = windowHeight;
        }

        protected override bool GetRootScreenRect(CefBrowser browser, ref CefRectangle rect)
        {
            return GetViewRect(browser, ref rect);
        }

        protected override bool GetScreenPoint(CefBrowser browser, int viewX, int viewY, ref int screenX, ref int screenY)
        {
            screenX = viewX;
            screenY = viewY;
            return true;
        }

        protected override bool GetViewRect(CefBrowser browser, ref CefRectangle rect)
        {
            rect.X = 0;
            rect.Y = 0;
            rect.Width = _windowWidth;
            rect.Height = _windowHeight;
            return true;
        }

        protected override bool GetScreenInfo(CefBrowser browser, CefScreenInfo screenInfo)
        {
            return false;
        }

        protected override void OnPopupSize(CefBrowser browser, CefRectangle rect)
        {
        }

        /*Called when an element should be painted. |type| indicates whether the element is the view or the popup widget. |buffer| contains the pixel data for the whole image. 
         * |dirtyRects| contains the set of rectangles that need to be repainted. 
         * On Windows |buffer| will be width*height*4 bytes in size and represents a BGRA image with an upper-left origin.
         *  The CefBrowserSettings.animation_frame_rate value controls the rate at which this method is called.*/
        protected override void OnPaint(CefBrowser browser, CefPaintElementType type, CefRectangle[] dirtyRects, IntPtr buffer, int width, int height)
        {
          
            //hard way
            if (MainBitmap == null)
            {
                _copysize = width*height*4; //32 bpp*stride(4)

               // var bmp=new Bitmap(width, height, width * 4, PixelFormat.Format32bppRgb, buffer);
                //Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                //bmp.LockBits(rect, ImageLockMode.ReadWrite,
                    //     PixelFormat.Format32bppRgb);

               // _copysize = bmpData.Stride * MainBitmap.Height

                MainBitmap = new byte[_copysize];
                
              
            }

            CurrentWidth = width;
            CurrentHeight = height;
            Marshal.Copy(buffer, MainBitmap, 0, _copysize);


            if(_memServer!=null)
                _memServer.WriteBytes(MainBitmap);


            
        }

    protected override void OnCursorChange(CefBrowser browser, IntPtr cursorHandle, CefCursorType type, CefCursorInfo customCursorInfo)
        {
            // MessageBox.Show("CURSOR");
        }
        
        protected override void OnScrollOffsetChanged(CefBrowser browser, double x, double y)
        {
        }

        
    }
}