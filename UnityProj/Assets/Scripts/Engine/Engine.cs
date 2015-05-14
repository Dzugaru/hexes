using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Engine
{
    public static class Engine
    {
        public static Player player;

        public static void Start()
        {
            WorldBlock.S = new WorldBlock(new HexXY(0, 0));
            WorldBlock.S.Generate(new BinaryNoiseFunc(new Vector2(100, 200), 0.25f, 0.6f),
                                new BinaryNoiseFunc(new Vector2(200, 100), 0.25f, 0.0f));
            player = new Player();
            player.Spawn(new HexXY(0, 0));

            //Overseer.Start();
           
        }

        public static void Update(float dt)
        {
            foreach (var ent in WorldBlock.S.entityList)            
                ent.Update(dt);            

            //Overseer.Update();
        }

        public static void PlayerMove(HexXY p)
        {
            if (WorldBlock.S.GetCellType(p) != TerrainCellType.Empty &&
               (!player.walker.dest.HasValue || p != player.walker.dest))
            {
                player.walker.SetDest(p, 10, false);
            }
        }

        public static void PlayerDrawRune(RuneType type, HexXY p)
        {
            player.DrawRune(type, p);
        }

        public static void PlayerEraseRune(HexXY p)
        {
            player.EraseRune(p);
        }
    }
}
