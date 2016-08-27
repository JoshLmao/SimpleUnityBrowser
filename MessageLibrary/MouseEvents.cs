using System;


namespace MessageLibrary
{
    public enum MouseEventType
    {
        Done = -1,
        ButtonDown = 0,
        ButtonUp = 1,
        Move = 2,
        Leave=3,
        Wheel=4,
       

    }

    public enum MouseButton
    {
        Left=0,
        Right=1,
        Middle=2,
        None=4
    }

    [Serializable]
    public class MouseMessage:AbstractEvent
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Delta { get; set; }
        public MouseEventType Type { get; set; }
        public MouseButton Button { get; set; }
       
    }
}
