using System;
using System.Collections.Generic;
using UnityEngine;

namespace Engine
{
    public class WorldBlock
    {
        //TODO: assume single worldBlock for now
        public static WorldBlock S;

        public static readonly int terrainTypesCount = Enum.GetValues(typeof(TerrainCellType)).Length;
        public const int sz = 32;
        public readonly HexXY position;

        //Terrain
        public TerrainCellType[,] cellTypes = new TerrainCellType[sz, sz];
        public int[] cellTypeCounts = new int[terrainTypesCount];
        public int nonEmptyCellsCount;

        public TerrainCellType GetCellType(HexXY pos)
        {
            if (pos.x >= 0 && pos.x < sz && pos.y >= 0 && pos.y < sz)
                return cellTypes[pos.x, pos.y];
            else
                return TerrainCellType.Empty;
        }

        //Entities
        public LList<Entity> entityList = new LList<Entity>();
        public LList<Entity>[,] entityMap = new LList<Entity>[sz,sz];

        //Pathfinding support
        public int pfExpandMarker = 0;
        public int[,] pfExpandMap = new int[sz, sz];
        public byte[,] pfStepsMap = new byte[sz, sz];
        public bool[,] pfBlockedMap = new bool[sz, sz]; //TODO: flags enum for layers of blocking (ground, air, astral, etc.?)        

        public bool pfIsPassable(HexXY pos)
        {
            return pos.x >= 0 && pos.x < sz && pos.y >= 0 && pos.y < sz &&
               GetCellType(pos) != TerrainCellType.Empty;
        }

        public int pfGetPassCost(HexXY pos)
        {
            return 1;
        }

        public void pfCalcStaticBlocking()
        {
            for (int x = 0; x < sz; x++)
            {
                for (int y = 0; y < sz; y++)
                {
                    pfBlockedMap[x, y] = cellTypes[x, y] == TerrainCellType.Empty;
                }
            }
        }

        public WorldBlock(HexXY position)
        {
            this.position = position;

            for (int x = 0; x < sz; x++)
                for (int y = 0; y < sz; y++)
                    entityMap[x, y] = new LList<Entity>();
        }

        //TODO:
        //static void dispatchToBlock(alias func, AA...)(HexXY globalPos, AA args)
        //    {
        //    HexXY localPos;
        //    auto blockX = globalPos.x < 0 ? (-globalPos.x - 1) / sz - 1 : globalPos.x / sz;
        //    auto blockY = globalPos.y < 0 ? (-globalPos.y - 1) / sz - 1 : globalPos.y / sz;
        //    localPos.x = globalPos.x - blockX * sz;
        //    localPos.y = globalPos.y - blockY * sz;
        //    //getBlock(blockX, blockY).func(localPos, args);
        //}

        //Generation
        public void Generate(BinaryNoiseFunc nonEmpty, BinaryNoiseFunc snow, bool cutEdges)
        {
            nonEmptyCellsCount = 0;
            for (int i = 0; i < cellTypeCounts.Length; i++)
                cellTypeCounts[i] = 0;

            for (int y = 0; y < sz; y++)
            {
                for (int x = 0; x < sz; x++)
                {
                    if (cutEdges && (x == 0 || y == 0 || x == sz - 1 || y == sz - 1)) continue;

                    HexXY c = position + new HexXY(x, y);
                    Vector2 p = c.ToPlaneCoordinates();
                    TerrainCellType type;
                    if (nonEmpty.Get(p))
                    {
                        if (snow.Get(p))
                        {
                            type = TerrainCellType.Grass;
                        }
                        else
                        {
                            type = TerrainCellType.DryGround;
                        }
                        ++nonEmptyCellsCount;
                    }
                    else
                    {
                        type = TerrainCellType.Empty;
                    }
                    cellTypes[x, y] = type;
                    ++cellTypeCounts[(int)type];
                }
            }

            pfCalcStaticBlocking();
        }

        public void GenerateSolidFirstType()
        {

            for (int i = 0; i < cellTypeCounts.Length; i++)
                cellTypeCounts[i] = 0;

            nonEmptyCellsCount = cellTypeCounts[1] = sz * sz;

            for (int y = 0; y < sz; y++)
                for (int x = 0; x < sz; x++)
                    cellTypes[x, y] = (TerrainCellType)1;
        }
    }
}

