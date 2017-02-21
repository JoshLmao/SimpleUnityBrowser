using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharedPluginServer;
using Xilium.CefGlue;

namespace CefHost
{
    static class Program
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            MessageBox.Show("START");
            log.Info("_____________HOST START");
            foreach (var arg in args)
            {
                log.Info("_______ARG:"+arg);
                MessageBox.Show(arg);
            }
            
            //  Application.EnableVisualStyles();
            // Application.SetCompatibleTextRenderingDefault(false);
            CefMainArgs cefMainArgs;
            cefMainArgs = new CefMainArgs(args);
            var cefApp = new WorkerCefApp(false);

            return CefRuntime.ExecuteProcess(cefMainArgs, cefApp, IntPtr.Zero);

        }
    }
}
