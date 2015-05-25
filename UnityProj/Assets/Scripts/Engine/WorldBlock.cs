using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Engine
{
    public class WorldBlock
    {
        public static readonly int terrainTypesCount = Enum.GetValues(typeof(TerrainCellType)).Length;
        public const int sz = 32;
        public readonly HexXY position;

        public HexXY Offset { get { return new HexXY(position.x * sz, position.y * sz); } }

        //Terrain
        TerrainCellType[,] cellTypes = new TerrainCellType[sz, sz];
        public int[] cellTypeCounts = new int[terrainTypesCount];       

        public TerrainCellType GetCellType(HexXY pos)
        {
            return cellTypes[pos.x, pos.y];            
        }

        public void SetCellType(HexXY pos, TerrainCellType type)
        {
            //Can't destroy tile under object
            if (entityMap[pos.x, pos.y].Count > 0 && type == TerrainCellType.Empty) return;

            var oldCellType = cellTypes[pos.x, pos.y];
            --cellTypeCounts[(int)oldCellType];
            cellTypes[pos.x, pos.y] = type;
            ++cellTypeCounts[(int)type];            
        }

        public static bool IsInRange(HexXY pos)
        {
            return pos.x >= 0 && pos.x < sz && pos.y >= 0 && pos.y < sz;
        }

        public static bool CanTryToMoveToBlockType(PFBlockType blockType)
        {
            return blockType == PFBlockType.DynamicBlocked ||
                    blockType == PFBlockType.Unblocked;
        }

        //Entities       
        public LList<Entity>[,] entityMap = new LList<Entity>[sz,sz];

        //Pathfinding support        
        public enum PFBlockType : byte
        {
            EdgeBlocked = 0,
            StaticBlocked = 1,            
            Unblocked = 100,            
            DynamicBlocked = 10,
            DoorBlocked = 11,
        }

        public int[,] pfExpandMap = new int[sz, sz];
        public byte[,] pfStepsMap = new byte[sz, sz];
        public PFBlockType[,] pfBlockedMap = new PFBlockType[sz, sz];

        public static int PFGetPassCost(HexXY pos)
        {
            return 1;
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
                {
                    writer.Write((byte)cellTypes[x, y]);
                    writer.Write((byte)pfBlockedMap[x, y]);
                }
        }

        public void SaveDynamicPart(BinaryWriter writer)
        {
            List<HexXY> nonZeroList = new List<HexXY>();
            for (int x = 0; x < sz; x++)
                for (int y = 0; y < sz; y++)
                {
                    int count = entityMap[x, y].Count;
                    if (count > 0) nonZeroList.Add(new HexXY(x, y));
                }

            writer.Write(nonZeroList.Count);
            foreach (var nz in nonZeroList)
            {
                int count = entityMap[nz.x, nz.y].Count;                
                writer.Write(nz.x);
                writer.Write(nz.y);
                writer.Write(count);
                foreach (var ent in entityMap[nz.x, nz.y])                
                    ent.Save(writer);                
            }        
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
                    wb.pfBlockedMap[x, y] = (PFBlockType)reader.ReadByte();
                }
            return wb;
        }

        public void LoadDynamicPart(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var p = new HexXY(reader.ReadInt32(), reader.ReadInt32());                
                int entCount = reader.ReadInt32();
                for (int j = 0; j < entCount; j++)                
                    Entity.Load(reader);                
            }            
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

            for (int x = 0; x < sz; x++)
                for (int y = 0; y < sz; y++)                
                    pfBlockedMap[x, y] = cellTypes[x, y] == TerrainCellType.Empty ? PFBlockType.EdgeBlocked : PFBlockType.Unblocked;               
            
        }      
    }
}

