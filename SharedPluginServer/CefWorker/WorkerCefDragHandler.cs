using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xilium.CefGlue;

namespace SharedPluginServer
{
    class WorkerCefDragHandler:CefDragHandler
    {

        protected override bool OnDragEnter(CefBrowser browser, CefDragData dragData, CefDragOperationsMask mask)
        {
            return false;
        }

        protected override void OnDraggableRegionsChanged(CefBrowser browser, CefDraggableRegion[] regions)
        {
            
        }
    }
}
