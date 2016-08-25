using System;


namespace MessageLibrary
{
  public enum KeyboardEventType
    {
        CharKey=0,
        Down=1,
        Up=2,
        Focus=3
    
    }

    [Serializable]
    public class KeyboardEvent : AbstractEvent
    {
        public KeyboardEventType Type;
        public int Key;
    }
}
