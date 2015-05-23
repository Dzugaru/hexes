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

        public Mob(MobData mobData) : base(EntityClass.Character, (uint)mobData.characterType)
        {
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
                if (!walker.dest.HasValue || walker.dest != E.player.pos)
                {
                    walker.SetDest(E.player.pos, 1, 0);
                }

                //Is we're blocked for some time try to find a way around other mobs	
                if (walker.isWalkBlocked && walker.walkBlockedTime > 0.5f) //TODO: random time?
                {
                    //log(format("%s %f", isWalkBlocked, walkBlockedTime));
                    walker.SetDest(E.player.pos, 1, 10);
                }

                //Attack
                if (!walker.isWalking && HexXY.Dist(pos, E.player.pos) == 1)
                {
                    isAttacking = true;
                    Interfacing.PerformInterfaceAttack(graphicsHandle, E.player.pos);
                    fibered.StartFiber(AttackFiber());                    
                }
            }

            base.Update(dt);
        }

        IEnumerator<float> AttackFiber()
        {
            var oldPlayerPos = E.player.pos;
            yield return attackDmgAppDelay;
            if (E.player.pos == oldPlayerPos)
            {
                //Apply dmg						
                float dmg = attackDamage;
                Interfacing.PerformInterfaceDamage(E.player.graphicsHandle, dmg);                
                //TODO: refresh bar including new dot speed? (same with dotheal)
            }

            yield return attackDur - attackDmgAppDelay;            
            isAttacking = false;
        }
    }
}
