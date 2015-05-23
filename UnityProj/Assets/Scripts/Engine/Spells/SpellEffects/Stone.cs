using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine.SpellEffects
{
    public class Stone : SpellEffect //TODO: , IHasHP 
    {        
        public Stone(float power) : base(SpellEffectType.Stone, power)
        {
            this.power = power;            
        }

        public override void Update(float dt)
        {
            power -= 0.1f * dt;                 
            if (power <= 0) Die();

            base.Update(dt);
        }

        public override void Spawn(HexXY p)
        {           
            base.Spawn(p);
            Level.S.SetPFBlockedMap(p, WorldBlock.PFBlockType.DynamicBlocked);
        }

        public override void Die()
        {
            base.Die();
            Level.S.SetPFBlockedMap(pos, WorldBlock.PFBlockType.Unblocked);
        }


        public override void StackOn(HexXY pos)
        {
            //Dont drop if path is already blocked by smth
            if (Level.S.GetPFBlockedMap(pos) == WorldBlock.PFBlockType.DynamicBlocked) return;
            

            //Kill all other effects
            foreach (var eff in Level.S.GetEntities(pos).Where(e => e is SpellEffect))
                eff.Die();            
            
            Spawn(pos);            
        }
    }
}
