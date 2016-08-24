using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageLibrary
{

    public enum EventType
    {
        Generic=0,
        Mouse=1,
        Keyboard=2
    }

    [Serializable]
    public class AbstractEvent
    {
        public EventType GenericType;//?
    }

    [Serializable]
    public class EventPacket
    {
        public EventType Type;

        public AbstractEvent Event;
    }

    public enum GenericEventType
    {
        Shutdown=0,
    }

    [Serializable]
    public class GenericEvent : AbstractEvent
    {
        public GenericEventType Type;
    }
}
