using Xilium.CefGlue;

namespace SharedPluginServer
{
    class WorkerCefApp : CefApp
    {
        private readonly WorkerCefRenderProcessHandler _renderProcessHandler;

        private bool _enableWebRtc = false;

        private bool _enableGPU = false;

        public WorkerCefApp(bool enableWebRtc,bool enableGPU)
        {
            _renderProcessHandler=new WorkerCefRenderProcessHandler();
            _enableWebRtc = enableWebRtc;

            _enableGPU = enableGPU;
        }

        protected override CefRenderProcessHandler GetRenderProcessHandler()
        {
            return _renderProcessHandler;
        }

        //GPU and others
        protected override void OnBeforeCommandLineProcessing(string processType, CefCommandLine commandLine)
        {
            if (string.IsNullOrEmpty(processType))
            {
                // commandLine.AppendSwitch("enable-webrtc");
                if (!_enableGPU)
                {
                    commandLine.AppendSwitch("disable-gpu");
                    commandLine.AppendSwitch("disable-gpu-compositing");
                }
                commandLine.AppendSwitch("enable-begin-frame-scheduling");
                commandLine.AppendSwitch("disable-smooth-scrolling");
                if (_enableWebRtc)
                {
                    commandLine.AppendSwitch("enable-media-stream", "true");
                }

                commandLine.AppendSwitch("enable-widevine-cdm", "true");
                commandLine.AppendSwitch("widevine-cdm-version", "1.4.8.866");
                commandLine.AppendSwitch("allow-running-insecure-content", "true");
                commandLine.AppendSwitch("enable-npapi", "true");
                commandLine.AppendSwitch("persist_session_cookies", "true");
                commandLine.AppendSwitch("enable-automatic-password-saving", "enable-automatic-password-saving");
                commandLine.AppendSwitch("enable-password-save-in-page-navigation", "enable-password-save-in-page-navigation");
            }
            //commandLine.AppendArgument("--enable-media-stream");
        }
    }
}