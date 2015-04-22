using System.Runtime.InteropServices;
using UnityEngine;


namespace Engine
{
    [StructLayout(LayoutKind.Sequential)]
    public struct HexXY
    {
        static Vector2 ex = new Vector2(Mathf.Sqrt(3) * 0.5f, 0.5f);
        static Vector2 ey = new Vector2(-Mathf.Sqrt(3) * 0.5f, 0.5f);
        static Vector2 iex = new Vector2(1f / Mathf.Sqrt(3), -1f / Mathf.Sqrt(3));
        static Vector2 iey = new Vector2(1, 1);

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

        public static HexXY FromPlaneCoordinates(Vector2 coords)
        {
            float x = coords.x * iex.x + coords.y * iey.x;
            float y = coords.x * iex.y + coords.y * iey.y;            
            int ix = Mathf.FloorToInt(x);
            int iy = Mathf.FloorToInt(y);
            Vector2 de = new Vector2(x - ix, y - iy);
            Vector2 d = de.x * ex + de.y * ey;
            float d00 = d.sqrMagnitude;
            float d10 = (ex - d).sqrMagnitude;
            float d01 = (ey - d).sqrMagnitude;
            float d11 = (ex + ey - d).sqrMagnitude;
            float mind = d00; int mini = 0;
            if (d10 < mind) { mind = d10; mini = 1; }
            if (d01 < mind) { mind = d01; mini = 2; }
            if (d11 < mind) { mind = d11; mini = 3; }
            switch (mini)
            {
                case 0: return new HexXY(ix, iy);
                case 1: return new HexXY(ix + 1, iy);
                case 2: return new HexXY(ix, iy + 1);
                case 3: return new HexXY(ix + 1, iy + 1);
                default: throw new System.InvalidProgramException();
            }
        }

        public static HexXY operator +(HexXY a, HexXY b)
        {
            return new HexXY(a.x + b.x, a.y + b.y);
        }

        public override string ToString()
        {
            return "(" + x + "," + y + ")";
        }
    }
}

