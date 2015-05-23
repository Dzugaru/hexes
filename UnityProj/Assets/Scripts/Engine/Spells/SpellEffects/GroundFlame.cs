using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine.SpellEffects
{
    public class GroundFlame : SpellEffect
    {
        public GroundFlame(float power) : base(SpellEffectType.GroundFlame, power)
        {
            this.power = power;              
        }

        public override void Update(float dt)
        {
            foreach (var ent in Level.S.GetEntities(pos).OfType<Mob>())            
                ent.hasHP.Damage(dt * 5.0f);            

            power -= 0.2f * dt;            
            if (power <= 0) Die();

            base.Update(dt);
        }        

        public override void StackOn(HexXY pos)
        {
            bool isExistsOther = Level.S.GetEntities(pos).Any(e => e is SpellEffect && !(e is GroundFlame));
            if (isExistsOther) return;
            var existingFlame = (GroundFlame)Level.S.GetEntities(pos).FirstOrDefault(e => e is GroundFlame);
            if (existingFlame != null)
            {
                if (existingFlame.power < power)
                    existingFlame.power = power;
            }
            else
            {
                Spawn(pos);
            }
        }
    }
}
