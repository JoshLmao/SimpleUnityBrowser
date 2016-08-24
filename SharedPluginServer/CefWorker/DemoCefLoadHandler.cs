using System.Windows.Forms;
using Xilium.CefGlue;

namespace SharedPluginServer
{
    class DemoCefLoadHandler : CefLoadHandler
    {
        public delegate void LoadFinished(int StatusCode);

        public event LoadFinished OnLoadFinished;

        protected override void OnLoadStart(CefBrowser browser, CefFrame frame)
        {
           
            // A single CefBrowser instance can handle multiple requests
            //   for a single URL if there are frames (i.e. <FRAME>, <IFRAME>).
           // if (frame.IsMain)
           // {
                //Console.WriteLine("START: {0}", browser.GetMainFrame().Url);
               // MessageBox.Show("____START:" + browser.GetMainFrame().Url);
           // }
        }

        protected override void OnLoadEnd(CefBrowser browser, CefFrame frame, int httpStatusCode)
        {
            if (frame.IsMain)
            {
                //   Console.WriteLine("END: {0}, {1}", browser.GetMainFrame().Url, httpStatusCode);
                //if (OnLoadFinished != null)
                OnLoadFinished?.Invoke(httpStatusCode);
            }
        }
    }
}