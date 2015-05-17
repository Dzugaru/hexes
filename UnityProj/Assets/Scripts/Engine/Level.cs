using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            Dictionary<HexXY, WorldBlock> all = new Dictionary<HexXY, WorldBlock>();

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

        WorldBlock GetBlockWithCell(HexXY p, bool shouldCreateIfNotFound, out HexXY localp)
        {
            localp = new HexXY(0, 0);

            var blockX = p.x < 0 ? (-p.x - 1) / WorldBlock.sz - 1 : p.x / WorldBlock.sz;
            var blockY = p.y < 0 ? (-p.y - 1) / WorldBlock.sz - 1 : p.y / WorldBlock.sz;
            var blockPos = new HexXY(blockX, blockY);

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

            localp.x = p.x - blockX * WorldBlock.sz;
            localp.y = p.y - blockY * WorldBlock.sz;
            return block;            
        }

        public void SetCell(HexXY p, TerrainCellType type)
        {

        }
    }
}
