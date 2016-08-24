using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharedPluginServer.Interprocess;

namespace SharedPluginServer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            //log4net.Config.XmlConfigurator.Configure();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            CefWorker worker=new CefWorker();
            worker.Init();
            SharedMemServer _server=new SharedMemServer();
            _server.Init(1);

           // NamedPipeServer.PipeName = "MainCommChannel";
           // ThreadStart pipeStart=new ThreadStart(NamedPipeServer.CreatePipeServer);

           /* Thread listenerThread=new Thread(pipeStart);
            listenerThread.SetApartmentState(ApartmentState.STA);
            listenerThread.IsBackground = true;
            listenerThread.Start();*/
            SocketServer ssrv=new SocketServer();
            ssrv.Init();


            Application.Run(new Form1(worker,_server,ssrv));

            ssrv.Shutdown();
        }
    }
}
