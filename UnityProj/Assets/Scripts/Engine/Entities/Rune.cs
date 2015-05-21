using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Engine
{
    public class Rune : Entity, IRotatable
    {
        public bool CanRotate { get { return Data.runeDatas[(RuneType)entityType].isDirectional; } }

        public Rune(RuneType type, uint dir) : base(EntityClass.Rune, (uint)type)
        {            
            this.dir = dir;
        }      

        public static bool CanDraw(Entity ent, RuneType rune, HexXY pos)
        {
            return 
                HexXY.Dist(ent.pos, pos) == 1 &&                
                !Level.S.GetEntities(pos).Any(e => e is Rune);

        }

        public static bool CanErase(Entity ent, HexXY pos)
        {
            return
                //HexXY.Dist(ent.pos, pos) <= 1 && //DEBUG
                Level.S.GetEntities(pos).Any(e => e is Rune);
        }

        public static void DrawRune(Entity ent, RuneType type, HexXY pos)
        {
            var runeData = Data.runeDatas[type];
            uint dirIdx;
            if (runeData.isDirectional)
                dirIdx = (uint)HexXY.neighbours.IndexOf(pos - ent.pos);
            else
                dirIdx = 0;

            var rune = new Rune(type, dirIdx);            
            rune.Spawn(pos);
        }

        public static void EraseRune(HexXY pos)
        {   
            var rune = Level.S.GetEntities(pos).First(e => e is Rune);
            rune.Die();
        }

        public override void Save(BinaryWriter writer)
        {
            base.Save(writer);
            writer.Write((byte)DerivedTypes.Rune);
            
            writer.Write((byte)dir);            
        }

        public static Rune Load(BinaryReader reader, uint type)
        {
            return new Rune((RuneType)type, reader.ReadByte());
        }
    }
}
