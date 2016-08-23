using Xilium.CefGlue;
using Xilium.CefGlue.Wrapper;

namespace SharedPluginServer
{
    class DemoCefApp : CefApp
    {
        private readonly DemoCefRenderProcessHandler _renderProcessHandler;

       

        public DemoCefApp()
        {
            _renderProcessHandler=new DemoCefRenderProcessHandler();

        }

       

      

        protected override CefRenderProcessHandler GetRenderProcessHandler()
        {
            return _renderProcessHandler;
        }
    }
}