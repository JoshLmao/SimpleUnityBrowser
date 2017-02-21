#define USE_ARGS

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using MessageLibrary;
using SharedPluginServer.Interprocess;
using Xilium.CefGlue;

namespace SharedPluginServer
{

    //Main application
    public class App
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        private SharedMemServer _memServer;

        private SharedCommServer _controlServer;

        private CefWorker _mainWorker;

        private System.Windows.Forms.Timer _exitTimer;

        private bool _mainProcess;

        /// <summary>
        /// App constructor
        /// </summary>
        /// <param name="worker">Main CEF worker</param>
        /// <param name="memServer">Shared memory file</param>
        /// <param name="commServer">TCP server</param>
        public App(CefWorker worker, SharedMemServer memServer, SharedCommServer commServer,bool mainProcess)
        {
            _memServer = memServer;
            _mainWorker = worker;
            _controlServer = commServer;
            _mainProcess = mainProcess;

            _mainWorker.SetMemServer(_memServer);

            //attach dialogs and queries
            _mainWorker.OnJSDialog += _mainWorker_OnJSDialog;
            _mainWorker.OnBrowserJSQuery += _mainWorker_OnBrowserJSQuery;

            //attach page events
            _mainWorker.OnPageLoaded += _mainWorker_OnPageLoaded;


            //change it by cycle?
            


           // SocketServer.OnReceivedMessage += HandleMessage;
           SharedCommServer.OnReceivedMessage += HandleMessage;

            log.Info("Setting up shutdown timer");
            _exitTimer = new Timer();
            _exitTimer.Interval = 20000;
            _exitTimer.Tick += _exitTimer_Tick;
            _exitTimer.Start();
        }

        private void _mainWorker_OnPageLoaded(string url, int status)
        {
            // log.Info("Navigated to:"+url);

            GenericEvent msg = new GenericEvent()
            {
                NavigateUrl = url,
                GenericType = BrowserEventType.Generic,
                Type = GenericEventType.PageLoaded
            };

            EventPacket ep = new EventPacket
            {
                Event = msg,
                Type = BrowserEventType.Generic
            };

            MemoryStream mstr = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(mstr, ep);

            _controlServer.SendData(mstr.GetBuffer());
        }

        //shut down by timer, in case of client crash/hang
        private void _exitTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                log.Info("Exiting by timer,timeout:" + _exitTimer.Interval);
                log.Info("==============SHUTTING DOWN==========");
               SharedCommServer.OnReceivedMessage -= HandleMessage;
                _mainWorker.Shutdown();

                _memServer.Dispose();

              //  _controlServer.Shutdown();

                if(_mainProcess)
                CefRuntime.Shutdown();


                Application.Exit();

            }
            catch (Exception ex)
            {

                log.Info("Exception on shutdown:" + ex.StackTrace);
            }
        }

        private void _mainWorker_OnBrowserJSQuery(string query)
        {
            GenericEvent msg = new GenericEvent()
            {
                JsQuery = query,
                GenericType = BrowserEventType.Generic,
                Type = GenericEventType.JSQuery
            };

            EventPacket ep = new EventPacket
            {
                Event = msg,
                Type = BrowserEventType.Generic
            };

            MemoryStream mstr = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(mstr, ep);

            _controlServer.SendData(mstr.GetBuffer());
        }

        private void _mainWorker_OnJSDialog(string message, string prompt, DialogEventType type)
        {
            DialogEvent msg = new DialogEvent()
            {
                DefaultPrompt = prompt,
                Message = message,
                Type = type,
                GenericType = BrowserEventType.Dialog
            };

            EventPacket ep = new EventPacket
            {
                Event = msg,
                Type = BrowserEventType.Dialog
            };

            MemoryStream mstr = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(mstr, ep);

            _controlServer.SendData(mstr.GetBuffer());
        }

        /// <summary>
        /// Main message handler
        /// </summary>
        /// <param name="msg">Message from client app</param>
        public void HandleMessage(EventPacket msg)
        {

            //reset timer
            _exitTimer.Stop();
            _exitTimer.Start();

            switch (msg.Type)
            {
                case BrowserEventType.Ping:
                {

                    break;
                }

                case BrowserEventType.Generic:
                {
                    GenericEvent genericEvent = msg.Event as GenericEvent;
                    if (genericEvent != null)
                    {
                        switch (genericEvent.Type)
                        {
                            case GenericEventType.Shutdown:
                            {
                                try
                                {
                                    log.Info("==============SHUTTING DOWN==========");
                                    SharedCommServer.OnReceivedMessage -= HandleMessage;
                                    _mainWorker.Shutdown();

                                    _memServer.Dispose();

                                            //_controlServer.Shutdown();
                                            _controlServer.isWorking = false;

                                            if (_mainProcess)
                                                CefRuntime.Shutdown();

                                    Application.Exit();

                                }
                                catch (Exception e)
                                {

                                    log.Info("Exception on shutdown:" + e.StackTrace);
                                }

                                break;
                            }
                            case GenericEventType.Navigate:

                                _mainWorker.Navigate(genericEvent.NavigateUrl);
                                break;

                            case GenericEventType.GoBack:
                                _mainWorker.GoBack();
                                break;

                            case GenericEventType.GoForward:
                                _mainWorker.GoForward();
                                break;

                            case GenericEventType.ExecuteJS:
                                _mainWorker.ExecuteJavaScript(genericEvent.JsCode);
                                break;

                            case GenericEventType.JSQueryResponse:
                            {
                                _mainWorker.AnswerQuery(genericEvent.JsQueryResponse);
                                break;
                            }

                        }
                    }
                    break;
                }

                case BrowserEventType.Dialog:
                {
                    DialogEvent de = msg.Event as DialogEvent;
                    if (de != null)
                    {
                        _mainWorker.ContinueDialog(de.success, de.input);
                    }
                    break;

                }

                case BrowserEventType.Keyboard:
                {
                    KeyboardEvent keyboardEvent = msg.Event as KeyboardEvent;
                    if (keyboardEvent != null)
                    {
                        if (keyboardEvent.Type != KeyboardEventType.Focus)
                            _mainWorker.KeyboardEvent(keyboardEvent.Key, keyboardEvent.Type);
                        else
                            _mainWorker.FocusEvent(keyboardEvent.Key);

                    }
                    break;
                }
                case BrowserEventType.Mouse:
                {
                    MouseMessage mouseMessage = msg.Event as MouseMessage;
                    if (mouseMessage != null)
                    {
                        switch (mouseMessage.Type)
                        {
                            case MouseEventType.ButtonDown:
                                _mainWorker.MouseEvent(mouseMessage.X, mouseMessage.Y, false, mouseMessage.Button);
                                break;
                            case MouseEventType.ButtonUp:
                                _mainWorker.MouseEvent(mouseMessage.X, mouseMessage.Y, true, mouseMessage.Button);
                                break;
                            case MouseEventType.Move:
                                _mainWorker.MouseMoveEvent(mouseMessage.X, mouseMessage.Y, mouseMessage.Button);
                                break;
                            case MouseEventType.Leave:
                                _mainWorker.MouseLeaveEvent();
                                break;
                            case MouseEventType.Wheel:
                                _mainWorker.MouseWheelEvent(mouseMessage.X, mouseMessage.Y, mouseMessage.Delta);
                                break;
                        }
                    }

                    break;
                }
            }


        }


    }




    static class Program
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The main entry point for the application.
        /// args:
        /// width,
        /// height,
        /// initialURL,
        /// memory file name,
        /// comm port,
        /// WebRTC?1:0
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {

            log.Info("===============START================");

            // if (args.Length > 0)
            /* {
                string msg = "";
                foreach (var s in args)
                {
                    msg = msg + ";" + s;
                }
                log.Info("ARGS:" + msg);

            }*/
            //args = new string[] { "--enable-media-stream" };


            int defWidth = 1280;
            int defHeight = 720;
            //string defUrl = "http://www.google.com";
            string defUrl = "http://test.webrtc.org";
            string defFileName = "MainSharedMem";
            int defPort = 18885;
            bool useWebRTC = false;

            string defPath = "";//or "E:\\temp\\cef"

            bool initAllStuff = true;

            if (args.Length > 0 && args[0] != "--type=renderer") //ok,so just for fun - do we need to init this all in case of a renderer?
            {


                if (args.Length > 1)
                {
                    defWidth = Int32.Parse(args[0]);
                    defHeight = Int32.Parse(args[1]);
                }
                if (args.Length > 2)
                    defUrl = args[2];
                if (args.Length > 3)
                    defFileName = args[3];
                if (args.Length > 4)
                    defPort = Int32.Parse(args[4]);
                if (args.Length > 5)
                    if (args[5] == "1")
                        useWebRTC = true;
                if (args.Length > 6)
                    defPath = args[6];
            }
            else
            {
               // initAllStuff = false;
                log.Info("Starting render process");
            }

           

                log.InfoFormat(
                    "Starting plugin, settings:width:{0},height:{1},url:{2},memfile:{3},port:{4},temp path:{5}",
                    defWidth, defHeight, defUrl, defFileName, defPort, defPath);

                bool runtimeStarted = false;

            
                try
                {

                    if (initAllStuff)
                    {
                        //////// CEF RUNTIME
                        try
                        {
                            CefRuntime.Load();
                        }
                        catch (DllNotFoundException ex)
                        {
                            log.ErrorFormat("{0} error", ex.Message);
                            return 1;
                        }
                        catch (CefRuntimeException ex)
                        {
                            log.ErrorFormat("{0} error", ex.Message);
                            return 2;
                        }
                        catch (Exception ex)
                        {
                            log.ErrorFormat("{0} error", ex.Message);
                            return 3;

                        }

                        log.Info("CEF Runtime loaded");

                        //I think, at this point we already have a runtime, so...

                    }


                    CefMainArgs cefMainArgs;
                        cefMainArgs = new CefMainArgs(args);
                        var cefApp = new WorkerCefApp(useWebRTC);

                        int exitCode = CefRuntime.ExecuteProcess(cefMainArgs, cefApp);
                        if (exitCode != -1)
                       
                        {
                            //TODO
                            //sometimes we have an error here, why?
                            log.ErrorFormat("CefRuntime could not the secondary process? code:" + exitCode);
                            //if(runtimeStarted)
                            //CefRuntime.Shutdown();
                            return exitCode;
                        }
                    
                    runtimeStarted = true;

                    log.Info("CEF Runtime started");
                    var cefSettings = new CefSettings
                    {
                        SingleProcess = false,
                        MultiThreadedMessageLoop = true,
                        WindowlessRenderingEnabled = true,
                        LogSeverity = CefLogSeverity.Info,
                        //TODO!!
                        CachePath = defPath, //"E:\\temp\\cef",
                        //UserDataPath
                        //LogFile
                        //BackgroundCOlor - ?
                       // BrowserSubprocessPath = "CefHost.exe"
                    };


                    if (initAllStuff)
                    {
                        try
                        {
                            CefRuntime.Initialize(cefMainArgs, cefSettings, cefApp, IntPtr.Zero);
                        }
                        catch (CefRuntimeException ex)
                        {
                            log.ErrorFormat("{0} error", ex.Message);
                           // if (runtimeStarted)
                           //     CefRuntime.Shutdown();
                            return 4;
                        }
                    }
                    /////////////
                }
                catch (Exception ex)
                {
                    log.Info("EXCEPTION ON CEF INITIALIZATION:" + ex.Message + "\n" + ex.StackTrace);
                    //throw;
                    if (runtimeStarted)
                        CefRuntime.Shutdown();
                    return 5;
                }
            


            CefWorker worker = new CefWorker();
                worker.Init(defWidth, defHeight, defUrl);

                log.Info("CEF worker initialized");

                SharedMemServer server = new SharedMemServer();
                try
                {

                    server.Init(defWidth*defHeight*4, defFileName);
                }
                catch (Exception ex)
                {

                    log.Info("EXCEPTION ON SHARED MEMORY INITIALIZATION:" + ex.Message + "\n" + ex.StackTrace);

                    CefRuntime.Shutdown();
                    return 6;
                    // throw;
                }

                log.Info("Shared memory initialized");

            // SocketServer ssrv = new SocketServer();
            SharedCommServer ssrv = new SharedCommServer();
                try
                {

                ssrv.Init("input_test", "output_test",true);
                 //   ssrv.Init(defPort);
                }
                catch (Exception ex)
                {
                    log.Info("EXCEPTION ON SOCKET INITIALIZATION:" + ex.Message + "\n" + ex.StackTrace);
                    CefRuntime.Shutdown();                    return 7;
                    //throw;
                }

                log.Info("Sockets initialized");



                var app = new App(worker, server, ssrv,initAllStuff);

                log.Info("All done,running an app!");
            
            Application.Run();

           
            CefRuntime.Shutdown();
            return 0;
        }
    }
}
