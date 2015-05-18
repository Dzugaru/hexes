using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Engine
{
    public class Level
    {
        class WorldBlocksCache
        {
            const int cacheMaxSz = 4;

            struct CacheEntry
            {
                public HexXY pos;
                public WorldBlock worldBlock;
            }

            CacheEntry[] cache = new CacheEntry[cacheMaxSz];
            int cacheSz = 0;
            public Dictionary<HexXY, WorldBlock> all = new Dictionary<HexXY, WorldBlock>();

            public WorldBlock GetNoCache(HexXY p)
            {
                WorldBlock block = null;
                all.TryGetValue(p, out block);
                return block;
            }

            public WorldBlock Get(HexXY p)
            {
                for (int i = 0; i < cacheSz; i++)
                    if (cache[i].pos == p) return cache[i].worldBlock;

                WorldBlock block = null;
                all.TryGetValue(p, out block);
                if (block != null)
                {
                    for (int i = 1; i < cacheMaxSz; i++)
                        cache[i] = cache[i - 1];
                    cache[0] = new CacheEntry() { pos = p, worldBlock = block };
                    cacheSz = Math.Min(cacheSz + 1, cacheMaxSz);
                }

                return block;
            }

            public void Add(HexXY p, WorldBlock block)
            {
                all.Add(p, block);
            }
        }


        WorldBlocksCache wbCache = new WorldBlocksCache();


        public void Update(float dt)
        {
            //TODO: update only active blocks (in radius?)
        }

        public WorldBlock GetBlock(HexXY blockp)
        {
            return wbCache.GetNoCache(blockp);
        }

        public static HexXY GetBlockCoords(HexXY p)
        {
            var blockX = p.x < 0 ? -((-p.x - 1) / WorldBlock.sz) - 1 : p.x / WorldBlock.sz;
            var blockY = p.y < 0 ? -((-p.y - 1) / WorldBlock.sz) - 1 : p.y / WorldBlock.sz;
            return new HexXY(blockX, blockY);
        }

        public static HexXY GetLocalCoords(HexXY p)
        {
            HexXY blockPos = GetBlockCoords(p);
            HexXY localp = new HexXY(0, 0);
            localp.x = p.x - blockPos.x * WorldBlock.sz;
            localp.y = p.y - blockPos.y * WorldBlock.sz;
            return localp;
        }

        public static HexXY GetBlockCoords(Vector2 coord)
        {
            return GetBlockCoords(HexXY.FromPlaneCoordinates(coord));
        }

        WorldBlock GetBlockWithCell(HexXY p, bool shouldCreateIfNotFound, out HexXY localp)
        {
            localp = new HexXY(0, 0);
            HexXY blockPos = GetBlockCoords(p);

            WorldBlock block = wbCache.Get(blockPos);
            if (block == null)
            {
                if (!shouldCreateIfNotFound) return null;
                else
                {
                    block = new WorldBlock(blockPos);
                    wbCache.Add(blockPos, block);
                }
            }

            localp.x = p.x - blockPos.x * WorldBlock.sz;
            localp.y = p.y - blockPos.y * WorldBlock.sz;
            return block;
        }

        public WorldBlock SetCellType(HexXY p, TerrainCellType type)
        {
            HexXY localPos;
            WorldBlock block = GetBlockWithCell(p, true, out localPos);
            block.SetCellType(localPos, type);
            return block;
        }

        public TerrainCellType GetCellType(HexXY p)
        {
            HexXY localPos;
            WorldBlock block = GetBlockWithCell(p, false, out localPos);
            if (block == null) return TerrainCellType.Empty;
            else return block.GetCellType(localPos);
        }

        public void SaveStaticPart(BinaryWriter writer)
        {
            writer.Write(wbCache.all.Count);
            foreach (var wb in wbCache.all.Values)            
                wb.SaveStaticPart(writer);
        }

        public static Level Load(BinaryReader reader)
        {
            var level = new Level();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var wb = WorldBlock.Load(reader);
                level.wbCache.Add(wb.position, wb);
            }
            return level;
        }
    }
}
