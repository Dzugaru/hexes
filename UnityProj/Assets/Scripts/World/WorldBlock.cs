using UnityEngine;

namespace Engine
{
    public class WorldBlock
    {
        public HexXY position { get; private set; }
        public int size { get; private set; }

        public TerrainCellType[,] cellTypes;

        public int[] cellTypeCounts;
        public int nonEmptyCellsCount;

        public WorldBlock(HexXY position, int size)
        {
            this.position = position;
            this.size = size;
            cellTypes = new TerrainCellType[size, size];
            cellTypeCounts = new int[World.TerrainCellTypesCount];            
        }

        public void Generate(BinaryNoiseFunc nonEmpty, BinaryNoiseFunc snow)
        {
            nonEmptyCellsCount = 0;
            for (int i = 0; i < cellTypeCounts.Length; i++)
            {
                cellTypeCounts[i] = 0;
            }


            for (int y = 0; y < size; ++y)
            {
                for (int x = 0; x < size; ++x)
                {
                    HexXY c = position + new HexXY(x, y);
                    Vector2 p = c.ToPlaneCoordinates(); 
                    TerrainCellType type;
                    if (nonEmpty.Get(p))
                    {                        
                        if (snow.Get(p))
                        {
                            type = TerrainCellType.Snow;
                        }
                        else
                        {
                            type = TerrainCellType.Grass;
                        }
                        ++nonEmptyCellsCount;
                    }
                    else
                    {
                        type = TerrainCellType.Empty;
                    }
                    cellTypes[y, x] = type;
                    ++cellTypeCounts[(int)type];
                }
            }

            //Debug.Log("Block generated: " + nonEmptyCellsCount + " non-empty cells");
            //for (int i = 1; i < World.TerrainCellTypesCount; i++)
            //{
            //   Debug.Log(((TerrainCellType)i).ToString() + " " + cellTypeCounts[i]);
            //}
        }
    }
}

