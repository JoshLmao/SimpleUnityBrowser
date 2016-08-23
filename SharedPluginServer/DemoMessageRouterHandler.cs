using Xilium.CefGlue;
using Xilium.CefGlue.Wrapper;

namespace SharedPluginServer
{
    class DemoMessageRouterHandler : CefMessageRouterBrowserSide.Handler
    {
        public override bool OnQuery(CefBrowser browser, CefFrame frame, long queryId, string request, bool persistent, CefMessageRouterBrowserSide.Callback callback)
        {
           
            callback.Success("OK");
            return true;
        }

        public override void OnQueryCanceled(CefBrowser browser, CefFrame frame, long queryId)
        {
        }
    }
}