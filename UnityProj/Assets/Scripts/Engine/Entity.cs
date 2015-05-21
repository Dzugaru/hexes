using System;
using System.Collections.Generic;
using System.IO;
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
        public uint dir; //TODO: Move to IRotatable interface?
        public Interfacing.EntityHandle graphicsHandle;
        public bool hasGraphicsHandle;
        public EntityClass entityClass;
        public uint entityType;

        public Entity(EntityClass cls, uint type)
        {
            components = new List<IEntityComponent>();
            if (this is IHasHP) { hasHP = new HasHP(this); components.Add(hasHP); }
            if (this is IWalker) { walker = new Walker(this, 64); components.Add(walker); }
            if (this is IFibered) { fibered = new Fibered(); components.Add(fibered); }

            entityType = type;
            entityClass = cls;
            hasGraphicsHandle = false;            
        }

        public T GetComponent<T>() where T : IEntityComponent
        {
            foreach (var comp in components)            
                if (comp is T) return (T)comp;

            throw new Tools.AssertException("No such component");            
        }       

        public virtual void Update(float dt)
        {
            //TODO: move this to Walker?
            HexXY prevPos = pos;

            foreach(var comp in components) if(!comp.OnUpdate(dt)) break;

            //TODO: move this to Walker? or it can be thrown by some other force...
            if (pos != prevPos)
            {
                bool removeSucceded = Level.S.RemoveEntity(prevPos, this);                
                Tools.Assert(removeSucceded);
                Level.S.AddEntity(pos, this);
            }
        }

        public virtual void Spawn(HexXY p)
        {
            if (!hasGraphicsHandle)            
                graphicsHandle = Interfacing.CreateEntity(this);                            

            pos = p;

            Level.S.entityList.Add(this);
            Level.S.AddEntity(p, this);

            foreach (var comp in components) comp.OnSpawn(p);

            Interfacing.PerformInterfaceSpawn(graphicsHandle, pos);
            UpdateInterface();
        }

        public virtual void UpdateInterface()
        {            
            if (this is IHasHP)
		    {
                Interfacing.PerformInterfaceUpdateHP(graphicsHandle, hasHP.currentHP, hasHP.maxHP);
            }

            IRotatable rotatable = this as IRotatable;
            if (rotatable != null)
            {
                if(rotatable.CanRotate)
                    Interfacing.PerformInterfaceUpdateRotation(graphicsHandle, dir);
            }
        }

        public virtual void Die()
        {
            foreach (var comp in components) comp.OnDie();            

            Level.S.entityList.Remove(this);
            Level.S.RemoveEntity(pos, this);
            Interfacing.PerformInterfaceDie(graphicsHandle);            
        }

        public bool Equals(Entity other)
        {
            return ReferenceEquals(this, other);
        }

        protected enum DerivedTypes : byte
        {
            Rune = 0,
            Mob = 1,
            SpellEffect = 2,
            StatueCaster = 3
        }      

        public virtual void Save(BinaryWriter writer)
        {   
            writer.Write(entityType);
            writer.Write(pos.x);
            writer.Write(pos.y);           
        }

        public static void Load(BinaryReader reader)
        {
            Entity ent;

            var type = reader.ReadUInt32();
            var pos = new HexXY(reader.ReadInt32(), reader.ReadInt32());
            
            var classType = (DerivedTypes)reader.ReadByte();
            switch (classType)
            {
                case DerivedTypes.Rune: ent = Rune.Load(reader, type); break;
                case DerivedTypes.StatueCaster: ent = StatueCaster.Load(reader); break;
                default: throw new NotImplementedException();
            }

            ent.Spawn(pos);            
        }
    }
}
