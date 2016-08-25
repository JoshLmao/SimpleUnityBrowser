using Xilium.CefGlue;
using Xilium.CefGlue.Wrapper;

namespace SharedPluginServer
{
    class WorkerCefApp : CefApp
    {
        private readonly WorkerCefRenderProcessHandler _renderProcessHandler;

       

        public WorkerCefApp()
        {
            _renderProcessHandler=new WorkerCefRenderProcessHandler();

        }

       

      

        protected override CefRenderProcessHandler GetRenderProcessHandler()
        {
            return _renderProcessHandler;
        }
    }
}