using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine.SpellEffects
{
    public class GroundFlame : SpellEffect
    {
        public float power;

        public void Construct()
        {
            power = 1;
            base.Construct(SpellEffectType.GroundFlame);
        }

        public override void Update(float dt)
        {
            power -= 0.2f * dt;
            UpdateInterface();
            if (power <= 0) Die();

            base.Update(dt);
        }

        public override void UpdateInterface()
        {
            Interfacing.PerformInterfaceUpdateSpellEffect(entityHandle, power);            
        }
    }
}
