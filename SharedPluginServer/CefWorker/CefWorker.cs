using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MessageLibrary;
using Xilium.CefGlue;
using Xilium.CefGlue.Wrapper;


namespace SharedPluginServer
{

    public class CefWorker:IDisposable
    {
        private static readonly log4net.ILog log =
    log4net.LogManager.GetLogger(typeof(CefWorker));

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        private WorkerCefClient _client;

        private static bool _initialized = false;

       

         public CefMessageRouterBrowserSide BrowserMessageRouter { get; private set; }



        #region IDisposable
        ~CefWorker()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                log.Info("=============SHUTTING DOWN========");
                Shutdown();
            }

        }
        #endregion

        public void Init(int width,int height,string starturl)
        {
            log.Info("___________INIT___________");
          


                

               

               RegisterMessageRouter();

                CefWindowInfo cefWindowInfo = CefWindowInfo.Create();
                cefWindowInfo.SetAsWindowless(IntPtr.Zero, false);
                var cefBrowserSettings = new CefBrowserSettings();

                cefBrowserSettings.JavaScript=CefState.Enabled;
                cefBrowserSettings.CaretBrowsing=CefState.Enabled;
                cefBrowserSettings.TabToLinks=CefState.Enabled;
                cefBrowserSettings.WebSecurity=CefState.Disabled;

            _client = new WorkerCefClient(width, height,this);
            //_client = new WorkerCefClient(1280, 720);
            //string url = "http://www.reddit.com/";

            string url = "http://www.yandex.ru/";
            if (starturl != "")
                url = starturl;
                CefBrowserHost.CreateBrowser(cefWindowInfo, _client, cefBrowserSettings, url);

                // MessageBox.Show("INITIALIZED");
                //Application.Idle += (s, e) => CefRuntime.DoMessageLoopWork();
                _initialized = true;
               // _client.OnLoadFinished += _client_OnLoadFinished;
            
        }

        public void SetMemServer(SharedMemServer memServer)
        {
            _client.SetMemServer(memServer);
        }

        private void RegisterMessageRouter()
        {
            if (!CefRuntime.CurrentlyOn(CefThreadId.UI))
            {
                PostTask(CefThreadId.UI, this.RegisterMessageRouter);
                return;
            }

            // window.cefQuery({ request: 'my_request', onSuccess: function(response) { console.log(response); }, onFailure: function(err,msg) { console.log(err, msg); } });
            BrowserMessageRouter = new CefMessageRouterBrowserSide(new CefMessageRouterConfig());
            BrowserMessageRouter.AddHandler(new WorkerMessageRouterHandler());
           //log.Info("BrowserMessageRouter created");
        }

        #region Task

        public static void PostTask(CefThreadId threadId, Action action)
        {
            CefRuntime.PostTask(threadId, new ActionTask(action));
        }

        internal sealed class ActionTask : CefTask
        {
            public Action _action;

            public ActionTask(Action action)
            {
                _action = action;
            }

            protected override void Execute()
            {
                _action();
                _action = null;
            }
        }

        public delegate void Action();

        #endregion

        

        public byte[] GetBitmap()
        {
            return _client.GetBitmap();
        }

        public int GetWidth()
        {
            return _client.GetWidth();
        }

        public int GetHeight()
        {
            return _client.GetWidth();
        }

        public void Shutdown()
        {
            _client.Shutdown();
          // 
        }

        public void Navigate(string url)
        {
            _client.Navigate(url);
        }

        public void ExecuteJavaScript(string jscode)
        {
            _client.ExecuteJavaScript(jscode);
        }

        public void MouseEvent(int x, int y,bool updown,MouseButton button)
        {
            _client.MouseEvent(x,y,updown,button);
        }

        public void MouseMoveEvent(int x, int y,MouseButton button)
        {
            _client.MouseMoveEvent(x, y,button);
        }

        public void KeyboardEvent(int character,KeyboardEventType type)
        {
            _client.KeyboardEvent(character,type);
        }

        public void FocusEvent(int focus)
        {
            _client.FocusEvent(focus);
        }

        public void MouseLeaveEvent()
        {
            _client.MouseLeaveEvent();
        }

        public void MouseWheelEvent(int x, int y, int delta)
        {
            _client.MouseWheelEvent(x,y,delta);
        }
        
    }
}
