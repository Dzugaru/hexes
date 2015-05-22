using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Engine
{
    public class StatueCaster : Entity, IClickable, IRotatable, IStaticBlocker
    {
        public bool CanRotate { get { return true; } }

        public HexXY sourceSpellPos;

        public StatueCaster(uint dir, HexXY sourceSpellPos) : base(EntityClass.Mech, (uint)MechType.StatueCaster)
        {
            this.dir = dir;
            this.sourceSpellPos = sourceSpellPos;            
        }

        public void Click()
        {
            var compileRune = (Rune)Level.S.GetEntities(sourceSpellPos).FirstOrDefault(e => e is Rune);            
            var spell = Spell.CompileSpell(compileRune, sourceSpellPos);
            spell.Cast(this, dir);            
        }

        public override void Save(BinaryWriter writer)
        {
            base.Save(writer);
            writer.Write((byte)DerivedTypes.StatueCaster);

            writer.Write((byte)dir);
            writer.Write(sourceSpellPos.x);
            writer.Write(sourceSpellPos.y);
        }

        public new static StatueCaster Load(BinaryReader reader)
        {
            return new StatueCaster(reader.ReadByte(), new HexXY(reader.ReadInt32(), reader.ReadInt32()));
        }
    }
}
