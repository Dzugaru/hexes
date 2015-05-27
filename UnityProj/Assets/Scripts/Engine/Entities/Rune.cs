using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Engine
{
    [GameSaveLoad("00000000")]
    public class Rune : Entity, IRotatable
    {
        public bool isLit, isUsedInSpell;
        public bool CanRotate { get { return Data.runeDatas[(RuneType)entityType].isDirectional; } }

        public Rune() : base(EntityClass.Rune)
        {

        }

        public Rune(uint dir) : base(EntityClass.Rune)
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
            var rune = Level.S.GetEntities(pos).OfType<Rune>().FirstOrDefault();
            return rune != null && !rune.isUsedInSpell &&
                   cellType != TerrainCellType.FusedRuneStone;
        }

        public override void Save(BinaryWriter writer)
        {
            base.Save(writer);            
            
            writer.Write((byte)dir);            
        }

        public override void LoadDerived(BinaryReader reader)
        {
            this.dir = reader.ReadByte();
        }
    }
}
