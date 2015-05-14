using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public class Rune : Entity
    {
        public uint dir;

        public void Construct(RuneType type, uint dir)
        {
            base.Construct(EntityClass.Rune, (uint)type);
            this.dir = dir;
        }

        public override void UpdateInterface()
        {
            Interfacing.PerformInterfaceUpdateRune(entityHandle, dir);
        }

        public static bool CanDraw(Entity ent, RuneType rune, HexXY pos)
        {
            return 
                HexXY.Dist(ent.pos, pos) == 1 &&                
                !WorldBlock.S.entityMap[pos.x, pos.y].Any(e => e is Rune);

        }

        public static bool CanErase(Entity ent, HexXY pos)
        {
            return
                //HexXY.Dist(ent.pos, pos) <= 1 && //DEBUG
                WorldBlock.S.entityMap[pos.x, pos.y].Any(e => e is Rune);
        }

        public static void DrawRune(Entity ent, RuneType type, HexXY pos)
        {
            var runeData = Data.runeDatas[type];
            uint dirIdx;
            if (runeData.isDirectional)
                dirIdx = (uint)HexXY.neighbours.IndexOf(pos - ent.pos);
            else
                dirIdx = 0;

            var rune = Freelist<Rune>.Allocate();
            rune.Construct(type, dirIdx);           
            rune.Spawn(pos);
        }

        public static void EraseRune(HexXY pos)
        {
            var entList = WorldBlock.S.entityMap[pos.x, pos.y];
            var rune = entList.First(e => e is Rune);
            rune.Die();
        }        
    }
}
