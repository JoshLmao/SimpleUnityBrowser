using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageLibrary
{
    public enum MouseEventType
    {
        Done = -1,
        LButtonDown = 0,
        LButtonUp = 1,
        Move = 2,

    }

    [Serializable]
    public struct MouseMessage
    {
        public int X { get; set; }
        public int Y { get; set; }
        public MouseEventType Type { get; set; }

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
                return hash;
            }
        }
    }
}
