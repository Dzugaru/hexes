using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine.SpellEffects
{
    public class Stone : SpellEffect //TODO: , IHasHP 
    {
        public float power;
        public Stone(float power) : base(SpellEffectType.Stone)
        {
            this.power = power;            
        }

        public override void Update(float dt)
        {
            power -= 0.1f * dt;
            Interfacing.PerformInterfaceUpdateSpellEffect(graphicsHandle, power);         
            if (power <= 0) Die();

            base.Update(dt);
        }

        public override void Spawn(HexXY p)
        {           
            base.Spawn(p);
            WorldBlock.S.pfBlockedMap[p.x, p.y] = true;
        }

        public override void Die()
        {
            base.Die();
            WorldBlock.S.pfBlockedMap[pos.x, pos.y] = false;
        }


        public override void StackOn(HexXY pos)
        {
            //Dont drop if path is already blocked by smth
            if (WorldBlock.S.pfBlockedMap[pos.x, pos.y]) return;
            

            //Kill all other effects
            foreach (var eff in WorldBlock.S.entityMap[pos.x, pos.y].Where(e => e is SpellEffect))            
            eff.Die();            
            
            Spawn(pos);            
        }
    }
}
