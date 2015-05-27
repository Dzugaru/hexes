using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine.LevelScripts
{
    public class LTest : LevelScript
    {
        public enum ID
        {
            PressurePlate01,
            Door01
        }

        public override HexXY PlayerSpawnPos
        {
            get
            {
                return new HexXY(-13, -16);
            }
        }

        public override void Update(float dt)
        {
            base.Update(dt);

            var plate01 = GetScriptEntity<PressPlate>(ID.PressurePlate01);
            if (plate01.isPressed)
            {
                fibers.StartFiber(UnpressPlateAfter(plate01, 2));
                foreach (var d in GetScriptEntities<Door>(ID.Door01))
                    d.OpenOrClose(true);
            }
        }

        public override void Start()
        {
            base.Start();

            //for (int i = 0; i < 0; i++)
            //{
            //    HexXY p;
            //    do
            //    {
            //        p.x = UnityEngine.Random.Range(-2 * WorldBlock.sz, 2 * WorldBlock.sz);
            //        p.y = UnityEngine.Random.Range(-2 * WorldBlock.sz, 2 * WorldBlock.sz);
            //    } while (Level.S.GetCellType(p) == TerrainCellType.Empty /*|| HexXY.Dist(player.pos, p) < 10*/ ||
            //    Level.S.GetPFBlockedMap(p) == WorldBlock.PFBlockType.DynamicBlocked);

            //    var mob = new Mob(Data.mobDatas["spider"]);
            //    mob.Spawn(p);
            //}
        }

        IEnumerator<float> UnpressPlateAfter(PressPlate plate, float t)
        {
            yield return t;
            plate.isPressed = false;
        }
    }
}
