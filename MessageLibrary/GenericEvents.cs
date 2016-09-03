using System;



namespace MessageLibrary
{

    public enum BrowserEventType
    {
        Ping=-1,
        Generic=0,
        Mouse=1,
        Keyboard=2,
        Dialog = 3
    }

    [Serializable]
    public class AbstractEvent
    {
        public BrowserEventType GenericType;//?
    }

    [Serializable]
    public class EventPacket
    {
        public BrowserEventType Type;

        public AbstractEvent Event;
    }

    public enum GenericEventType
    {
        Shutdown=0,
        Navigate=1,
        GoBack=2,
        GoForward=3,
        ExecuteJS=4,
        JSQuery=5,
        JSQueryResponse=6,
        PageLoaded=7

        
    }

   

    [Serializable]
    public class GenericEvent : AbstractEvent
    {
        public GenericEventType Type;

        public string NavigateUrl;

        public string JsCode;

        public string JsQuery;

        public string JsQueryResponse;
    }
}
