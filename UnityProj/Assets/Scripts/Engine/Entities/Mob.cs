using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public class Mob : Entity, IHasHP, IWalker, IFibered
    {
        float attackDmgAppDelay, attackDur, attackDamage;
        bool isAttacking;

        public void Construct(MobData mobData)
        {
            Construct(EntityClass.Character, (uint)mobData.characterType);

            walker.SetSpeed(mobData.speed);
            attackDmgAppDelay = mobData.attackDmgAppDelay;
            attackDur = mobData.attackDur;
            attackDamage = mobData.attackDamage;
            hasHP.maxHP = hasHP.currentHP = mobData.maxHP;
            isAttacking = false;
        }

        public override void Update(float dt)
        {
            if (!isAttacking)
            {
                //Simple walk to player
                if (!walker.dest.HasValue || walker.dest != Engine.player.pos)
                {
                    walker.SetDest(Engine.player.pos, 0, true);
                }

                //Is we're blocked for some time try to find a way around other mobs	
                if (walker.isWalkBlocked && walker.walkBlockedTime > 0.5f) //TODO: random time?
                {
                    //log(format("%s %f", isWalkBlocked, walkBlockedTime));
                    walker.SetDest(Engine.player.pos, 10, true);
                }

                //Attack
                if (!walker.isWalking && HexXY.Dist(pos, Engine.player.pos) == 1)
                {
                    isAttacking = true;
                    Interfacing.PerformInterfaceAttack(entityHandle, Engine.player.pos);
                    fibered.StartFiber(AttackFiber());                    
                }
            }

            base.Update(dt);
        }

        IEnumerator<float> AttackFiber()
        {
            var oldPlayerPos = Engine.player.pos;
            yield return attackDmgAppDelay;
            if (Engine.player.pos == oldPlayerPos)
            {
                //Apply dmg						
                float dmg = attackDamage;
                Interfacing.PerformInterfaceDamage(Engine.player.entityHandle, dmg);                
                //TODO: refresh bar including new dot speed? (same with dotheal)
            }

            yield return attackDur - attackDmgAppDelay;            
            isAttacking = false;
        }
    }
}
