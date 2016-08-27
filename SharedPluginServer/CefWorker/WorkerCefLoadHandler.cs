using Xilium.CefGlue;

namespace SharedPluginServer
{
    class WorkerCefLoadHandler : CefLoadHandler
    {

        public delegate void LoadFinished(int StatusCode);

        public event LoadFinished OnLoadFinished;

        protected override void OnLoadStart(CefBrowser browser, CefFrame frame)
        {
           
        }

        protected override void OnLoadEnd(CefBrowser browser, CefFrame frame, int httpStatusCode)
        {
            //TODO: check if this used at all
            
            if (frame.IsMain)
            {
                OnLoadFinished?.Invoke(httpStatusCode);
            }
        }
    }
}