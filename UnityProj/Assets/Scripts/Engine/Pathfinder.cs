using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public static class Pathfinder
    {
        const int pfMaxFrontSizeLog = 13; //8192

        struct Step
        {
            public readonly int dx, dy;
            public readonly byte backIdx;
            public Step(int dx, int dy, byte backIdx)
            {
                this.dx = dx;
                this.dy = dy;
                this.backIdx = backIdx;
            }

            public static HexXY operator +(HexXY lhs, Step rhs)
            {
                return new HexXY(lhs.x + rhs.dx, lhs.y + rhs.dy);
            }
        }

        struct XYCost : IComparable<XYCost>
        {
            public HexXY p;
            public uint len;
            public uint cost, sumCostHeuristic;          

            public int CompareTo(XYCost rhs)
            {
                return sumCostHeuristic <= rhs.sumCostHeuristic ? -1 : 1;
            }

            public XYCost(HexXY p, uint len, uint cost, uint sumCostHeuristic)
            {
                this.p = p;
                this.len = len;
                this.cost = cost;
                this.sumCostHeuristic = sumCostHeuristic;
            }
        }

        static readonly Step[] steps = new[] {
                new Step(1, 0, 3), new Step(1, 1, 4), new Step(0, 1, 5), new Step(-1, 0, 0), new Step(-1, -1, 1), new Step(0, -1, 2)
            };

        static BinaryHeap<XYCost> front = new BinaryHeap<XYCost>(pfMaxFrontSizeLog);

        static uint GetHeuristic(HexXY pos, HexXY to)
        {
            return HexXY.Dist(pos, to);
        }

        public static uint? FindPath(HexXY from, HexXY to, HexXY[] pathStorage, uint dynBlockCost = 0)
        {
            front.Reset();
            front.Enqueue(new XYCost(from, 0, 0, GetHeuristic(from, to)));

            //TODO: assume we're in the single worldblock for now
            ++Level.S.pfExpandMarker;
            Level.S.SetPFExpandMap(from, Level.S.pfExpandMarker);

            XYCost c;
            bool isFound = false;

            do
            {
                c = front.Dequeue();                

                if (c.p == to)
                {
                    isFound = true;
                    break;
                }

                foreach (var st in steps)
                {
                    var np = c.p + st;
                    var blockType = Level.S.GetPFBlockedMap(np);
                    if (blockType != WorldBlock.PFBlockType.StaticBlocked &&
                        Level.S.GetPFExpandMap(np) < Level.S.pfExpandMarker)
                    {
                        Level.S.SetPFExpandMap(np, Level.S.pfExpandMarker);
                        uint cost = (uint)(c.cost + WorldBlock.PFGetPassCost(np));

                        if (dynBlockCost > 0 && np != to && blockType == WorldBlock.PFBlockType.DynamicBlocked)
                            cost += dynBlockCost;

                        var n = new XYCost(np, c.len + 1, cost, cost + GetHeuristic(np, to));
                        front.Enqueue(n);
                        Level.S.SetPFStepsMap(np, st.backIdx);
                    }
                }
            } while (front.Count > 0);

            if (isFound && c.len <= pathStorage.Length)
            {
                uint pathLen = c.len;
                HexXY p = c.p;
                for (int i = 0; i < pathLen; i++)
                {
                    pathStorage[pathLen - i - 1] = p;
                    var backIdx = Level.S.GetPFStepsMap(p);
                    p = p + steps[backIdx];
                }
                return pathLen;
            }
            else
            {
                return null;
            }
        }
    }
}
