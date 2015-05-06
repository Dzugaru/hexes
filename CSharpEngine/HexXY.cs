using UnityEngine;

namespace Engine
{
    public struct HexXY
    {
        static readonly Vector2 ex = new Vector2(Mathf.Sqrt(3f) * 0.5f, 0.5f);
        static readonly Vector2 ey = new Vector2(-Mathf.Sqrt(3f) * 0.5f, 0.5f);
        static readonly HexXY[] neighbours = new[] { new HexXY(1, 0), new HexXY(1, 1), new HexXY(0, 1), new HexXY(-1, 0), new HexXY(-1, -1), new HexXY(0, -1) };

        public int x, y;

        public HexXY(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static uint dist(HexXY a, HexXY b)
        {
            int x = a.x - b.x;
            int y = a.y - b.y;
            if ((x < 0) == (y < 0))
                return (uint)Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
            else
                return (uint)Mathf.Abs(x - y);
        }

        public Vector2 toPlaneCoordinates()
        {
            return x * ex + y * ey;
        }

        public static HexXY operator +(HexXY lhs, HexXY rhs)
        {
            return new HexXY(lhs.x + rhs.x, lhs.y + rhs.y);
        }

        public static HexXY operator -(HexXY lhs, HexXY rhs)
        {
            return new HexXY(lhs.x - rhs.x, lhs.y - rhs.y);
        }

        public override bool Equals(object obj)
        {
            HexXY tobj = (HexXY)obj;
            return tobj == this;
        }

        public override int GetHashCode()
        {
            return (x << 16) + y;
        }

        public static bool operator ==(HexXY lhs, HexXY rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y;
        }

        public static bool operator !=(HexXY lhs, HexXY rhs)
        {
            return !(lhs == rhs);
        }

        public override string ToString()
        {
            return string.Format("({0},{1})", x, y);
        }
    }
}
