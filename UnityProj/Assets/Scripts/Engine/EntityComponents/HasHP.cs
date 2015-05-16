using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public interface IHasHP
    {
      
    }

    public class HasHP : IEntityComponent
    {
        Entity entity;

        public float currentHP, maxHP;

        public HasHP(Entity entity)
        {
            this.entity = entity;
        }

        public void Damage(float dmg)
        {
            currentHP = Math.Max(0, currentHP - dmg);
            Interfacing.PerformInterfaceDamage(entity.graphicsHandle, dmg);
            entity.UpdateInterface();
        }

        public bool OnUpdate(float dt)
        {
            if (currentHP == 0)
            {
                entity.Die();
                return false;
            }
            else
            {
                return true;
            }
        }

        public void OnDie()
        {
            
        }

        public void OnSpawn(HexXY pos)
        {
            
        }
    }
}
