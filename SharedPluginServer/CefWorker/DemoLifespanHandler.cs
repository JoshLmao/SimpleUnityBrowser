using Xilium.CefGlue;

namespace SharedPluginServer
{
    class DemoLifespanHandler : CefLifeSpanHandler
    {
        public CefBrowser MainBrowser;
        public CefBrowserHost MainBrowserHost;
        protected override void OnAfterCreated(CefBrowser browser)
        {
            MainBrowser = browser;
            MainBrowserHost = browser.GetHost();
        }
        protected override bool DoClose(CefBrowser browser)
        {
            // TODO: dispose core
            return false;
        }

        protected override void OnBeforeClose(CefBrowser browser)
        {
            CefWorker.BrowserMessageRouter.OnBeforeClose(browser);
        }
    }
}