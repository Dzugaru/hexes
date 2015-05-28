using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Engine
{    
    public abstract class Statue : Entity, IClickable, IRotatable, ICaster
    {
        public bool CanRotate { get { return true; } }       

        public bool isCasting;
        

        public HexXY sourceSpellPos;        

        public Statue() : base(EntityClass.Mech)
        {
            
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



        public abstract void Click();  
        

        public override void Save(BinaryWriter writer)
        {
            base.Save(writer); 
           
            writer.Write((byte)dir);
            writer.Write(sourceSpellPos.x);
            writer.Write(sourceSpellPos.y);
        }

        public override void LoadDerived(BinaryReader reader)
        {
            dir = reader.ReadByte();
            sourceSpellPos = new HexXY(reader.ReadInt32(), reader.ReadInt32());
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

        public abstract bool CanBeClicked { get; }
    }
}
