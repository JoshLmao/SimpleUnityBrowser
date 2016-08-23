using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xilium.CefGlue;
using Xilium.CefGlue.Wrapper;

namespace SharedPluginServer
{

    public class CefWorker
    {
        private static readonly log4net.ILog log =
    log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        private DemoCefClient _client;

        private static bool _initialized = false;

        public delegate void LoadFinished(int StatusCode);

        public event LoadFinished OnLoadFinished;

        public static CefMessageRouterBrowserSide BrowserMessageRouter { get; private set; }

        public void Init()
        {
            if (!_initialized)
            {
                // MessageBox.Show("_______INIT");
                try
                {
                    CefRuntime.Load();
                }
                catch (DllNotFoundException ex)
                {
                    //MessageBox.Show(ex.Message, "Error!");
                    //  return 1;
                    log.ErrorFormat("{0} error", ex.Message);
                }
                catch (CefRuntimeException ex)
                {
                    log.ErrorFormat("{0} error", ex.Message);
                    //  return 2;
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("{0} error", ex.Message);

                }


                var cefMainArgs = new CefMainArgs(new string[0]);
                var cefApp = new DemoCefApp();
                if (CefRuntime.ExecuteProcess(cefMainArgs, cefApp) != -1)
                {
                   log.ErrorFormat("CefRuntime could not the secondary process.");
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
                    log.ErrorFormat("{0} error", ex.Message);

                }

                RegisterMessageRouter();

                CefWindowInfo cefWindowInfo = CefWindowInfo.Create();
                cefWindowInfo.SetAsWindowless(IntPtr.Zero, false);
                var cefBrowserSettings = new CefBrowserSettings();

                cefBrowserSettings.JavaScript=CefState.Enabled;
                cefBrowserSettings.CaretBrowsing=CefState.Enabled;
                cefBrowserSettings.TabToLinks=CefState.Enabled;
                cefBrowserSettings.WebSecurity=CefState.Disabled;

                _client = new DemoCefClient(1280, 720);
                string url = "http://www.reddit.com/";
                CefBrowserHost.CreateBrowser(cefWindowInfo, _client, cefBrowserSettings, url);

                // MessageBox.Show("INITIALIZED");
               // Application.Idle += (s, e) => CefRuntime.DoMessageLoopWork();
                _initialized = true;
                _client.OnLoadFinished += _client_OnLoadFinished;
            }
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
            BrowserMessageRouter.AddHandler(new DemoMessageRouterHandler());
            log.Info("BrowserMessageRouter created");
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

        private void _client_OnLoadFinished(int StatusCode)
        {
            OnLoadFinished?.Invoke(StatusCode);
        }

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
            CefRuntime.Shutdown();
        }

        public void Navigate(string url)
        {
            _client.Navigate(url);
        }

        public void MouseEvent(int x, int y,bool updown)
        {
            _client.MouseEvent(x,y,updown);
        }

        public void MouseMoveEvent(int x, int y)
        {
            _client.MouseMoveEvent(x, y);
        }
    }
}
