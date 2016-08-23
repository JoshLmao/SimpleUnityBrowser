using System.Drawing;
using System.Windows.Forms;
using Xilium.CefGlue;

namespace SharedPluginServer
{
    class DemoCefClient : CefClient
    {
        private static readonly log4net.ILog log =
  log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly DemoCefLoadHandler _loadHandler;
        private readonly DemoCefRenderHandler _renderHandler;
        private readonly DemoLifespanHandler _lifespanHandler;
        private readonly WebRequestHandler _requestHandler;


        public delegate void LoadFinished(int StatusCode);

        public event LoadFinished OnLoadFinished;

        public DemoCefClient(int windowWidth, int windowHeight)
        {
            _renderHandler = new DemoCefRenderHandler(windowWidth, windowHeight);
            _loadHandler = new DemoCefLoadHandler();
            _loadHandler.OnLoadFinished += _loadHandler_OnLoadFinished;
            _lifespanHandler=new DemoLifespanHandler();
            _requestHandler=new WebRequestHandler();

           
        }

        protected override CefRequestHandler GetRequestHandler()
        {
            return _requestHandler;
        }

        

      

        private void _loadHandler_OnLoadFinished(int StatusCode)
        {
            OnLoadFinished?.Invoke(StatusCode);
        }

        protected override CefRenderHandler GetRenderHandler()
        {
            //  MessageBox.Show("GetRenderHandler");
            return _renderHandler;
        }

        protected override bool OnProcessMessageReceived(CefBrowser browser, CefProcessId sourceProcess,
            CefProcessMessage message)
        {
            var handled = CefWorker.BrowserMessageRouter.OnProcessMessageReceived(browser, sourceProcess, message);
            if (handled) return true;

            return false;
        }


        protected override CefLoadHandler GetLoadHandler()
        {
            return _loadHandler;
        }

        protected override CefLifeSpanHandler GetLifeSpanHandler()
        {
            return _lifespanHandler;
        }

        public byte[] GetBitmap()
        {
            return _renderHandler.MainBitmap;
        }

        public int GetWidth()
        {
            return _renderHandler.CurrentWidth;
        }

        public int GetHeight()
        {
            return _renderHandler.CurrentHeight;
        }

        public void Navigate(string url)
        {
            _lifespanHandler.MainBrowser.GetMainFrame().LoadUrl(url);
        }

        public void MouseEvent(int x,int y,bool updown)
        {
            //_lifespanHandler.MainBrowserHost.SendFocusEvent(true);
            _lifespanHandler.MainBrowser.GetHost().SendFocusEvent(true);
            CefMouseEvent mouseEvent = new CefMouseEvent()
            {
                X =x,
                Y =y,
            };
            CefEventFlags modifiers = new CefEventFlags();
            modifiers |= CefEventFlags.LeftMouseButton;
            mouseEvent.Modifiers = modifiers;
           log.Info("CLICK:" + x + "," + y);
            _lifespanHandler.MainBrowser.GetHost().SendMouseClickEvent(mouseEvent,CefMouseButtonType.Left, updown,1);
            
        }

        public void MouseMoveEvent(int x, int y)
        {
            CefMouseEvent mouseEvent = new CefMouseEvent()
            {
                X = x,
                Y = y,
            };
            _lifespanHandler.MainBrowser.GetHost().SendMouseMoveEvent(mouseEvent,false);
        }
    }
}