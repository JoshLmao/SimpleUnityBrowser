using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MessageLibrary;
using SharedPluginServer.Interprocess;
using Xilium.CefGlue;
using System.Windows;

namespace SharedPluginServer
{

    //just a glue
    public class App
    {
        private static readonly log4net.ILog log =
 log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        private SharedMemServer _memServer;

        private SocketServer _controlServer;

        private CefWorker _mainWorker;

        public App(CefWorker worker, SharedMemServer memServer, SocketServer commServer)
        {
            _memServer = memServer;
            _mainWorker = worker;
            _controlServer = commServer;

            _mainWorker.SetMemServer(_memServer);

            SocketServer.OnReceivedMessage += HandleMessage;
        }

        public void HandleMessage(EventPacket msg)
        {
          //  log.Info("________Packet:" + msg.Type);

            switch (msg.Type)
            {
                case EventType.Generic:
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
                                    //log.Info("___MAIN");
                                     _memServer.Dispose();
                                   log.Info("___MEM");
                                    _controlServer.Shutdown();
                                     log.Info("___CONTROL");
                                            
                                            Application.Exit();
                                  // Environment.Exit(Environment.ExitCode);
                                }
                                catch (Exception e)
                                {

                                    log.Info("______EXIT:"+e.StackTrace);
                                }

                                break;
                            }
                        }
                    }
                    break;
                }

                case EventType.Keyboard:
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
                case EventType.Mouse:
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
        /// initialURL
        /// </summary>
        [STAThread]
        static void Main(string[] args)
       // static void Main()
        {

            int defWidth = 1280;
            int defHeight = 720;
            string defUrl = "http://www.yandex.ru";


            log.Info("ARGS:"+args.Length);
          

           
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ////////RUNTIME
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

            //WARNING: process command line AFTER initialization

            if (args.Length > 1)
            {
                
                  defWidth = Int32.Parse(args[0]);
                 defHeight=Int32.Parse(args[1]);
                log.Info("width:"+defWidth+",height:"+defHeight);
            }
            if (args.Length > 2)
                defUrl = args[2];


            CefWorker worker =new CefWorker();
           worker.Init(defWidth,defHeight,defUrl);
            SharedMemServer _server=new SharedMemServer();
            _server.Init(defWidth*defHeight*4);

           
            SocketServer ssrv=new SocketServer();
            ssrv.Init();

            var app=new App(worker,_server,ssrv);
           // var app = new App(null, _server, ssrv);

            Application.Run();

            // Application.Run(new Form1(worker,_server,ssrv));

            //ssrv.Shutdown();
            CefRuntime.Shutdown();

        }
    }
}
