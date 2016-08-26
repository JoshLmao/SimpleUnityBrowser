using Xilium.CefGlue;

namespace SharedPluginServer
{
    class WorkerWebRequestHandler : CefRequestHandler
    {
        private readonly CefWorker _mainWorker;

        public WorkerWebRequestHandler(CefWorker mainCefWorker)
        {
            _mainWorker = mainCefWorker;
        }

        protected override bool OnBeforeBrowse(CefBrowser browser, CefFrame frame, CefRequest request, bool isRedirect)
        {
            _mainWorker.BrowserMessageRouter.OnBeforeBrowse(browser, frame);
            return base.OnBeforeBrowse(browser, frame, request, isRedirect);
        }

        protected override void OnRenderProcessTerminated(CefBrowser browser, CefTerminationStatus status)
        {
            _mainWorker.BrowserMessageRouter.OnRenderProcessTerminated(browser);
        }
    }
}