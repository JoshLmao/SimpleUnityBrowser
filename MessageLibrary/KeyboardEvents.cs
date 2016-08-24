using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageLibrary
{
  public enum KeyboardEventType
    {
        CharKey=0,
        Down=1,
        Up=2
    
    }

    [Serializable]
    public class KeyboardEvent : AbstractEvent
    {
        public KeyboardEventType Type;
        public int Key;
    }
}
