using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public abstract class Entity : IEquatable<Entity>
    {
        List<IEntityComponent> components;

        public Walker walker;
        public HasHP hasHP;
        public Fibered fibered;

        public HexXY pos;
        public Interfacing.EntityHandle entityHandle;
        public uint entityType;

        public Entity()
        {
            components = new List<IEntityComponent>();
            if (this is IHasHP) { hasHP = new HasHP(this); components.Add(hasHP); }
            if (this is IWalker) { walker = new Walker(this, 64); components.Add(walker); }
            if (this is IFibered) { fibered = new Fibered(); components.Add(fibered); }
        }

        public T GetComponent<T>() where T : IEntityComponent
        {
            foreach (var comp in components)            
                if (comp is T) return (T)comp;

            throw new Tools.AssertException("No such component");            
        }

        public void Construct(EntityClass cls, uint type)
        {
            entityHandle = Interfacing.CreateEntity(cls, type);
            entityType = type;
        }

        public virtual void Update(float dt)
        {
            //TODO: move this to Walker?
            HexXY prevPos = pos;

            foreach(var comp in components) if(!comp.OnUpdate(dt)) break;

            //TODO: move this to Walker? or it can be thrown by some other force...
            if (pos != prevPos)
            {
                bool removeSucceded = WorldBlock.S.entityMap[prevPos.x, prevPos.y].Remove(this);                
                Tools.Assert(removeSucceded);
                WorldBlock.S.entityMap[pos.x, pos.y].Add(this);
            }
        }

        public void Spawn(HexXY p)
        {
            pos = p;

            WorldBlock.S.entityList.Add(this);
            WorldBlock.S.entityMap[p.x, p.y].Add(this);

            foreach (var comp in components) comp.OnSpawn(p);

            Interfacing.PerformInterfaceSpawn(entityHandle, pos);
            UpdateInterface();
        }

        public virtual void UpdateInterface()
        {            
            if (this is IHasHP)
		    {
                Interfacing.PerformInterfaceUpdateHP(entityHandle, hasHP.currentHP, hasHP.maxHP);
            }
        }

        public void Die()
        {
            foreach (var comp in components) comp.OnDie();            

            WorldBlock.S.entityList.Remove(this);
            WorldBlock.S.entityMap[pos.x, pos.y].Remove(this);
            Interfacing.PerformInterfaceDie(entityHandle);
        }

        public bool Equals(Entity other)
        {
            return ReferenceEquals(this, other);
        }
    }
}
