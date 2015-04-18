using UnityEngine;


namespace Engine
{
    public struct HexXY
    {
        static Vector2 ex = new Vector2(Mathf.Sqrt(3) * 0.5f, 0.5f);
        static Vector2 ey = new Vector2(-Mathf.Sqrt(3) * 0.5f, 0.5f);

        public int x, y;

        public HexXY(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static float DistSqr(HexXY a, HexXY b)
        {
            return a.x * a.x + b.y * b.y - a.x * a.y;
        }

        public Vector2 ToPlaneCoordinates()
        {
            return x * ex + y * ey;
        }

        public static HexXY operator +(HexXY a, HexXY b)
        {
            return new HexXY(a.x + b.x, a.y + b.y);
        }
    }
}

