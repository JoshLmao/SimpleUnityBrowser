using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xilium.CefGlue;
using System.Drawing;
using System.Drawing.Imaging;


namespace ConsoleTest
{
    class Program
    {
        private static DemoCefClient _client;
        static void Main(string[] args)
        {
            try
            {
                CefRuntime.Load();
            }
            catch (DllNotFoundException ex)
            {
              //  MessageBox.Show(ex.Message, "Error!");
                //  return 1;
            }
            catch (CefRuntimeException ex)
            {
               // MessageBox.Show(ex.Message, "Error!");
                //  return 2;
            }
            catch (Exception ex)
            {
               // MessageBox.Show(ex.ToString(), "Error!");

            }


            var cefMainArgs = new CefMainArgs(new string[0]);
            var cefApp = new DemoCefApp();
            if (CefRuntime.ExecuteProcess(cefMainArgs, cefApp) != -1)
            {
                Console.Error.WriteLine("CefRuntime could not the secondary process.");
            }
            var cefSettings = new CefSettings
            {
                SingleProcess = false,
                MultiThreadedMessageLoop = true,
                //WindowlessRenderingEnabled = true,
                LogSeverity = CefLogSeverity.Info
            };

            try
            {
                CefRuntime.Initialize(cefMainArgs, cefSettings, cefApp, IntPtr.Zero);
            }
            catch (CefRuntimeException ex)
            {
              //  MessageBox.Show(ex.ToString(), "Error!");

            }
            CefWindowInfo cefWindowInfo = CefWindowInfo.Create();
             cefWindowInfo.SetAsWindowless(IntPtr.Zero, false);
            var cefBrowserSettings = new CefBrowserSettings();
            _client = new DemoCefClient(1280, 720);
            string url = "http://www.reddit.com/";
            CefBrowserHost.CreateBrowser(cefWindowInfo, _client, cefBrowserSettings, url);

            Console.WriteLine("Press a key at any time to end the program.");
            Console.ReadKey();

            // Clean up CEF.
            CefRuntime.Shutdown();
        }
    }

    class DemoCefApp : CefApp
    {
    }

    class DemoCefClient : CefClient
    {
        private readonly DemoCefLoadHandler _loadHandler;
        private readonly DemoCefRenderHandler _renderHandler;

        public DemoCefClient(int windowWidth, int windowHeight)
        {
            _renderHandler = new DemoCefRenderHandler(windowWidth, windowHeight);
            _loadHandler = new DemoCefLoadHandler();
        }

        protected override CefRenderHandler GetRenderHandler()
        {
            //MessageBox.Show("GetRenderHandler");
            return _renderHandler;
        }

        protected override CefLoadHandler GetLoadHandler()
        {
            return _loadHandler;
        }

       // public Bitmap GetBitmap()
       // {
       //     return _renderHandler.MainBitmap;
       // }
    }

    class DemoCefLoadHandler : CefLoadHandler
    {
        protected override void OnLoadStart(CefBrowser browser, CefFrame frame)
        {

            // A single CefBrowser instance can handle multiple requests
            //   for a single URL if there are frames (i.e. <FRAME>, <IFRAME>).
            if (frame.IsMain)
            {
                Console.WriteLine("START: {0}", browser.GetMainFrame().Url);
            }
        }

        protected override void OnLoadEnd(CefBrowser browser, CefFrame frame, int httpStatusCode)
        {
            if (frame.IsMain)
            {
                   Console.WriteLine("END: {0}, {1}", browser.GetMainFrame().Url, httpStatusCode);
            }
        }
    }

    class DemoCefRenderHandler : CefRenderHandler
    {
        private readonly int _windowHeight;
        private readonly int _windowWidth;

    //    public Bitmap MainBitmap = null;

        public DemoCefRenderHandler(int windowWidth, int windowHeight)
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

        protected override void OnPaint(CefBrowser browser, CefPaintElementType type, CefRectangle[] dirtyRects, IntPtr buffer, int width, int height)
        {
            // Save the provided buffer (a bitmap image) as a PNG.
            // if (MainBitmap == null)
            // {
            //  MainBitmap = new Bitmap(width, height, width*4, PixelFormat.Format32bppRgb, buffer);
            // MessageBox.Show("BITMAP CREATED");
            // }
            // bitmap.Save("LastOnPaint.png", ImageFormat.Png);
            Console.WriteLine("RENDER");
            var bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppRgb, buffer);
            bitmap.Save("LastOnPaint.png", ImageFormat.Png);
        }

        protected override void OnCursorChange(CefBrowser browser, IntPtr cursorHandle, CefCursorType type, CefCursorInfo customCursorInfo)
        {
        }

        protected override void OnScrollOffsetChanged(CefBrowser browser, double x, double y)
        {
        }
    }
}
