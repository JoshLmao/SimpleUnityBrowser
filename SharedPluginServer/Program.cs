using System;
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

        private SocketServer _controlServer;

        private CefWorker _mainWorker;

        /// <summary>
        /// App constructor
        /// </summary>
        /// <param name="worker">Main CEF worker</param>
        /// <param name="memServer">Shared memory file</param>
        /// <param name="commServer">TCP server</param>
        public App(CefWorker worker, SharedMemServer memServer, SocketServer commServer)
        {
            _memServer = memServer;
            _mainWorker = worker;
            _controlServer = commServer;

            _mainWorker.SetMemServer(_memServer);

            //attach dialogs and queries
            _mainWorker.OnJSDialog += _mainWorker_OnJSDialog;
            _mainWorker.OnBrowserJSQuery += _mainWorker_OnBrowserJSQuery;

            SocketServer.OnReceivedMessage += HandleMessage;
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

            _controlServer.Client.SendData(mstr.GetBuffer());
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

            _controlServer.Client.SendData(mstr.GetBuffer());
        }

        /// <summary>
        /// Main message handler
        /// </summary>
        /// <param name="msg">Message from client app</param>
        public void HandleMessage(EventPacket msg)
        {
          
            switch (msg.Type)
            {
                case BrowserEventType.Ping:
                {
                 
                        break;
                }

                case BrowserEventType.Generic:
                {
                    GenericEvent genericEvent=msg.Event as GenericEvent;
                    if (genericEvent != null)
                    {
                        switch (genericEvent.Type)
                        {
                             case GenericEventType.Shutdown:
                            {
                                try
                                {
                                    log.Info("==============SHUTTING DOWN==========");
                                    SocketServer.OnReceivedMessage -= HandleMessage;
                                       _mainWorker.Shutdown();
                                    
                                     _memServer.Dispose();
                                  
                                    _controlServer.Shutdown();
                                     
                                            
                                            Application.Exit();
                                  
                                }
                                catch (Exception e)
                                {

                                    log.Info("Exception on shutdown:"+e.StackTrace);
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
                        DialogEvent de=msg.Event as DialogEvent;
                    if (de != null)
                    {
                        _mainWorker.ContinueDialog(de.success,de.input);
                    }
                    break;
                    
                }

                case BrowserEventType.Keyboard:
                {
                        KeyboardEvent keyboardEvent=msg.Event as KeyboardEvent;
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
                        MouseMessage mouseMessage=msg.Event as MouseMessage;
                        if (mouseMessage != null)
                        {
                            switch (mouseMessage.Type)
                            {
                                case MouseEventType.ButtonDown:
                                    _mainWorker.MouseEvent(mouseMessage.X, mouseMessage.Y, false,mouseMessage.Button);
                                    break;
                                case MouseEventType.ButtonUp:
                                    _mainWorker.MouseEvent(mouseMessage.X, mouseMessage.Y, true,mouseMessage.Button);
                                    break;
                                case MouseEventType.Move:
                                    _mainWorker.MouseMoveEvent(mouseMessage.X, mouseMessage.Y, mouseMessage.Button);
                                    break;
                                    case MouseEventType.Leave:
                                    _mainWorker.MouseLeaveEvent();
                                    break;
                                    case MouseEventType.Wheel:
                                    _mainWorker.MouseWheelEvent(mouseMessage.X,mouseMessage.Y,mouseMessage.Delta);
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
        /// comm port
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //Application.EnableVisualStyles();
           // Application.SetCompatibleTextRenderingDefault(false);

            //////// CEF RUNTIME
            try
            {
                CefRuntime.Load();
            }
            catch (DllNotFoundException ex)
            {
                log.ErrorFormat("{0} error", ex.Message);
            }
            catch (CefRuntimeException ex)
            {
                log.ErrorFormat("{0} error", ex.Message);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("{0} error", ex.Message);

            }

            var cefMainArgs = new CefMainArgs(new string[0]);
            var cefApp = new WorkerCefApp();
            if (CefRuntime.ExecuteProcess(cefMainArgs, cefApp) != -1)
            {
                log.ErrorFormat("CefRuntime could not the secondary process.");
                return;
            }
            var cefSettings = new CefSettings
            {
                SingleProcess = false,
                MultiThreadedMessageLoop = true,
                WindowlessRenderingEnabled = true,
                LogSeverity = CefLogSeverity.Info,

            };



            try
            {
                CefRuntime.Initialize(cefMainArgs, cefSettings, cefApp, IntPtr.Zero);
            }
            catch (CefRuntimeException ex)
            {
                log.ErrorFormat("{0} error", ex.Message);

            }
            /////////////

            //WARNING: process command line AFTER initialization

            int defWidth = 1280;
            int defHeight = 720;
            string defUrl = "http://www.google.com";
            string defFileName = "MainSharedMem";
            int defPort = 8885;
         

            if (args.Length > 1)
            {
                defWidth = Int32.Parse(args[0]);
                 defHeight=Int32.Parse(args[1]);
                log.Info("width:"+defWidth+",height:"+defHeight);
            }
            if (args.Length > 2)
                defUrl = args[2];
            if (args.Length > 3)
                defFileName = args[3];
            if (args.Length > 4)
                defPort = Int32.Parse(args[4]);

            log.InfoFormat("Starting plugin, settings: width:{0},height:{1},url:{2},memfile:{3},port:{4}",
                defWidth,defHeight,defUrl,defFileName,defPort);


            CefWorker worker =new CefWorker();
           worker.Init(defWidth,defHeight,defUrl);
            SharedMemServer server=new SharedMemServer();
            server.Init(defWidth*defHeight*4,defFileName);

           
            SocketServer ssrv=new SocketServer();
            ssrv.Init(defPort);

            var app=new App(worker,server,ssrv);
          
            Application.Run();

            CefRuntime.Shutdown();

        }
    }
}
