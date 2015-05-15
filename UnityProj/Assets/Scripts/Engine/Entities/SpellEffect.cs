using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public class SpellEffect : Entity, IFibered
    {
        public void Construct(SpellEffectType type)
        {
            base.Construct(EntityClass.SpellEffect, (uint)type);            
        }
    }
}
