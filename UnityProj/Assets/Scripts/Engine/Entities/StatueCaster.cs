using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Engine
{
    public class StatueCaster : Entity, IClickable, IRotatable, ICaster
    {
        public bool CanRotate { get { return true; } }

        public enum BehaviorType : byte
        {
            CastingSpell = 0,
            TeachingMeleeSpell = 1,
            TeachingRangedSpell = 2        
        }

        public bool isCasting;
        public SpellExecuting exSpell;


        public HexXY sourceSpellPos;
        public BehaviorType behType;        

        public StatueCaster(BehaviorType type) : base(EntityClass.Mech)
        {
            behType = type;
            switch (behType)
            {
                case BehaviorType.CastingSpell: entityType = (uint)MechType.StatueCaster; break;
                case BehaviorType.TeachingMeleeSpell: entityType = (uint)MechType.StatueTeachMelee; break;
            }             
        }

        public override void Spawn(HexXY p)
        {
            base.Spawn(p);
            if(!G.S.isEditMode) Level.S.SetPFBlockedMap(p, WorldBlock.PFBlockType.StaticBlocked);
        }

        public override void Die()
        {
            if (!G.S.isEditMode) Level.S.SetPFBlockedMap(pos, WorldBlock.PFBlockType.Unblocked);
            base.Die();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            if (exSpell != null && !exSpell.isExecuting)
                exSpell = null;
            
            isCasting = exSpell != null;
        }

        public void Click()
        {
            switch (behType)
            {
                case BehaviorType.CastingSpell: ClickCaster(); break;
                case BehaviorType.TeachingMeleeSpell: ClickTeacher(); break;
            }
        }

        void ClickTeacher()
        {

        }

        void ClickCaster()
        {
            if (!isCasting)
            {
                var compileRune = (Rune)Level.S.GetEntities(sourceSpellPos).FirstOrDefault(e => e is Rune);
                var spell = Spell.CompileSpell(compileRune, sourceSpellPos);
                exSpell = spell.CastMelee(this, dir);
            }
            else
            {
                exSpell.Die();
                exSpell = null;
            }
        }

        public override void Save(BinaryWriter writer)
        {
            base.Save(writer);
            writer.Write((byte)DerivedTypes.StatueCaster);

            writer.Write((byte)behType);
            writer.Write((byte)dir);
            writer.Write(sourceSpellPos.x);
            writer.Write(sourceSpellPos.y);
        }

        public new static StatueCaster Load(BinaryReader reader)
        {
            var behType = (BehaviorType)reader.ReadByte();
            return new StatueCaster(behType)
            {                
                dir = reader.ReadByte(),
                sourceSpellPos = new HexXY(reader.ReadInt32(), reader.ReadInt32())
            };
        }

        public bool SpendMana(float amount)
        {
            return true;
        }

        public float Mana
        {
            get
            {
                return float.MaxValue;
            }
        }
    }
}
