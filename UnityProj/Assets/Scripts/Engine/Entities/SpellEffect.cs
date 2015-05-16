using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public abstract class SpellEffect : Entity, IFibered
    {
        public SpellEffect(SpellEffectType type) : base(EntityClass.SpellEffect, (uint)type)
        {
            
        }

        public abstract void StackOn(HexXY pos);
    }
}
