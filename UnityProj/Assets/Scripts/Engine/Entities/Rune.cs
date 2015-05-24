using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Engine
{
    public class Rune : Entity, IRotatable
    {
        public bool isLit;
        public bool CanRotate { get { return Data.runeDatas[(RuneType)entityType].isDirectional; } }

        public Rune(RuneType type, uint dir) : base(EntityClass.Rune, (uint)type)
        {            
            this.dir = dir;
        }      

        public static bool CanDraw(Entity ent, RuneType rune, HexXY pos)
        {
            var cellType = Level.S.GetCellType(pos);
            return cellType == TerrainCellType.RuneStone;
        }

        public static bool CanErase(Entity ent, HexXY pos)
        {
            var cellType = Level.S.GetCellType(pos);
            return Level.S.GetEntities(pos).Any(e => e is Rune) &&
                   cellType != TerrainCellType.FusedRuneStone;
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
