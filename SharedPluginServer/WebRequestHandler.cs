using Xilium.CefGlue;

namespace SharedPluginServer
{
    class WebRequestHandler : CefRequestHandler
    {
        protected override bool OnBeforeBrowse(CefBrowser browser, CefFrame frame, CefRequest request, bool isRedirect)
        {
            CefWorker.BrowserMessageRouter.OnBeforeBrowse(browser, frame);
            return base.OnBeforeBrowse(browser, frame, request, isRedirect);
        }

        protected override void OnRenderProcessTerminated(CefBrowser browser, CefTerminationStatus status)
        {
            CefWorker.BrowserMessageRouter.OnRenderProcessTerminated(browser);
        }
    }
}