using Xilium.CefGlue;

namespace SharedPluginServer
{
    class WorkerLifespanHandler : CefLifeSpanHandler
    {
        public CefBrowser MainBrowser;
        public CefBrowserHost MainBrowserHost;

        private readonly CefWorker _mainWorker;

        public WorkerLifespanHandler(CefWorker mainCefWorker)
        {
            
        }

        protected override void OnAfterCreated(CefBrowser browser)
        {
            MainBrowser = browser;
            MainBrowserHost = browser.GetHost();
        }
       protected override bool DoClose(CefBrowser browser)
        {

            // return false;
            return false;
        }

        protected override void OnBeforeClose(CefBrowser browser)
        {
            _mainWorker.BrowserMessageRouter.OnBeforeClose(browser);
        }
    }
}