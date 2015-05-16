using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine.SpellEffects
{
    public class GroundFlame : SpellEffect
    {
        public float power;

        public GroundFlame(float power) : base(SpellEffectType.GroundFlame)
        {
            this.power = power;              
        }

        public override void Update(float dt)
        {
            foreach (var ent in WorldBlock.S.entityMap[pos.x, pos.y].OfType<Mob>())            
                ent.hasHP.Damage(dt * 5.0f);            

            power -= 0.2f * dt;
            UpdateInterface();
            if (power <= 0) Die();

            base.Update(dt);
        }

        public override void UpdateInterface()
        {
            Interfacing.PerformInterfaceUpdateSpellEffect(graphicsHandle, power);            
        }

        public override void StackOn(HexXY pos)
        {
            bool isExistsOther = WorldBlock.S.entityMap[pos.x, pos.y].Any(e => e is SpellEffect && !(e is GroundFlame));
            if (isExistsOther) return;
            var existingFlame = (GroundFlame)WorldBlock.S.entityMap[pos.x, pos.y].FirstOrDefault(e => e is GroundFlame);
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
