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

        public static bool operator ==(MouseMessage m1, MouseMessage m2)
        {
            return m1.Equals(m2);
        }

        public static bool operator !=(MouseMessage m1, MouseMessage m2)
        {
            return !m1.Equals(m2);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MouseMessage))
                return false;
            MouseMessage cmp = (MouseMessage)obj;

            return (cmp.X == X) && (cmp.Y == Y) && (cmp.Type == Type);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + X.GetHashCode();
                hash = hash * 23 + Y.GetHashCode();
                hash = hash * 23 + Type.GetHashCode();
                hash = hash * 23 + Delta.GetHashCode();
                hash = hash * 23 + Button.GetHashCode();
                return hash;
            }
        }
    }
}
