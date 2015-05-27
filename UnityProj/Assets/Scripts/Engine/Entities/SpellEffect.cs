using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    [GameSaveLoad("00000002")]
    public abstract class SpellEffect : Entity, IFibered
    {
        public float power;

        public SpellEffect(SpellEffectType type, float power) : base(EntityClass.SpellEffect, (uint)type)
        {
            this.power = power;
        }

        public abstract void StackOn(HexXY pos);
    }
}
