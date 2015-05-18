using System;
using System.Collections.Generic;
using System.IO;
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
        TerrainCellType[,] cellTypes = new TerrainCellType[sz, sz];
        public int[] cellTypeCounts = new int[terrainTypesCount];       

        public TerrainCellType GetCellType(HexXY pos)
        {
            return cellTypes[pos.x, pos.y];            
        }

        public void SetCellType(HexXY pos, TerrainCellType type)
        {
            var oldCellType = cellTypes[pos.x, pos.y];
            --cellTypeCounts[(int)oldCellType];
            cellTypes[pos.x, pos.y] = type;
            ++cellTypeCounts[(int)type];
        }

        public bool IsInRange(HexXY pos)
        {
            return pos.x >= 0 && pos.x < sz && pos.y >= 0 && pos.y < sz;
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

        public void SaveStaticPart(BinaryWriter writer)
        {
            writer.Write(position.x);
            writer.Write(position.y);
            for (int x = 0; x < sz; x++)
                for (int y = 0; y < sz; y++)
                    writer.Write((byte)cellTypes[x, y]);
        }

        public static WorldBlock Load(BinaryReader reader)
        {
            var pos = new HexXY(reader.ReadInt32(), reader.ReadInt32()); 

            var wb = new WorldBlock(pos);
            for (int x = 0; x < sz; x++)
                for (int y = 0; y < sz; y++)
                {
                    var type = (TerrainCellType)reader.ReadByte();
                    wb.cellTypes[x, y] = type;
                    ++wb.cellTypeCounts[(int)type];
                }
            return wb;
        }

        public void SaveDynamicPart(BinaryWriter writer)
        {
            //TODO: this should save entities and pfBlockedMap etc.
            throw new NotImplementedException();
        }

        public void LoadDynamicPart(BinaryReader reader)
        {
            //TODO: this should load entities and pfBlockedMap etc.
            throw new NotImplementedException();
        }

        //Procedural generation
        public void Generate(BinaryNoiseFunc nonEmpty, BinaryNoiseFunc snow, bool cutEdges)
        {           
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
    }
}

