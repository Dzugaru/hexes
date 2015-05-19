using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Engine
{
    public static class E
    {
        public static Player player;
        public static LevelScript levelScript;  
        

        public static void LoadLevel(string levelName)
        {
            var levelDataAsset = (TextAsset)Resources.Load("Levels/" + levelName);
            using (var reader = new System.IO.BinaryReader(new System.IO.MemoryStream(levelDataAsset.bytes)))
            {
                Level.Load(reader);
                Level.S.LoadDynamicPart(reader);
            }

            levelScript = (LevelScript)Activator.CreateInstance(Assembly.GetExecutingAssembly().GetTypes().First(t => t.Namespace == "Engine.LevelScripts" && t.Name == levelName));

            player = new Player();
            player.Spawn(levelScript.PlayerSpawnPos);

            //for (int i = 0; i < 0; i++)
            //{
            //    HexXY p;
            //    do
            //    {
            //        p.x = UnityEngine.Random.Range(0, WorldBlock.sz);
            //        p.y = UnityEngine.Random.Range(0, WorldBlock.sz);
            //    } while (WorldBlock.S.GetCellType(p) == TerrainCellType.Empty || HexXY.Dist(player.pos, p) < 10 ||
            //    WorldBlock.S.pfBlockedMap[p.x,p.y]);

            //    var mob = new Mob(Data.mobDatas["spider"]);
            //    mob.Spawn(p);
            //}

            //Overseer.Start();
        }

        public static void Update(float dt)
        {
            foreach (var ent in Level.S.entityList)
                ent.Update(dt);

            Spell.Update(dt);
            //Overseer.Update();
        }

        public static void PlayerMove(HexXY p)
        {
            if (Level.S.GetPFBlockedMap(p) != WorldBlock.PFBlockType.StaticBlocked &&
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

        public static bool PlayerCompileSpell(HexXY p)
        {
            return player.CompileSpell(p);
        }

        public static void PlayerCastSpell(HexXY p)
        {
            player.CastCurrentSpell(p);
        }
    }
}
