using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public abstract class LevelScript
    {
        protected Fibered fibers;

        public abstract HexXY PlayerSpawnPos { get; }

        public Dictionary<HexXY, List<uint>> triggerZones = new Dictionary<HexXY, List<uint>>();

        public virtual void Update(float dt)
        {
            fibers.OnUpdate(dt);
        }

        public virtual void Start()
        {
            fibers = new Fibered();            
        }

        protected Entity GetScriptEntity<TEnum>(TEnum idEnum) where TEnum : struct, IConvertible
        {
            return (Entity)Level.S.entityList.OfType<IHasScriptID>().FirstOrDefault(e => e.ID == EnumConverter<TEnum>.ConvertToUInt(idEnum));
        }

        protected IEnumerable<Entity> GetScriptEntities<TEnum>(TEnum idEnum) where TEnum : struct, IConvertible
        {
            foreach (Entity scrEnt in Level.S.entityList.OfType<IHasScriptID>().Where(e => e.ID == EnumConverter<TEnum>.ConvertToUInt(idEnum)))
                yield return scrEnt;
        }

        public IEnumerable<TEnum> GetTriggerZonesAt<TEnum>(HexXY p) where TEnum : struct, IConvertible
        {
            List<uint> zones;
            if (!triggerZones.TryGetValue(p, out zones)) yield break;           
            foreach (var z in zones)
                yield return EnumConverter<TEnum>.ConvertFromUInt(z);            
        }

        public bool IsPlayerAtTriggerZone<TEnum>(TEnum zone) where TEnum : struct, IConvertible
        {
            return GetTriggerZonesAt<TEnum>(E.player.pos).Contains(zone);
        }

        public IEnumerable<uint> GetTriggerZonesAt(HexXY p)
        {
            List<uint> zones;
            if (!triggerZones.TryGetValue(p, out zones)) yield break;
            foreach (var z in zones)
                yield return z;
        }

        public void AddTriggerZoneTo(HexXY p, uint zone)
        {
            List<uint> l;
            if (!triggerZones.TryGetValue(p, out l))
            {
                l = new List<uint>();
                triggerZones.Add(p, l);
            }
            l.Add(zone);
        }

        public void RemoveTriggerZoneFrom(HexXY p, uint zone)
        {
            triggerZones[p].Remove(zone);
        }

        public virtual void LoadStaticPart(System.IO.BinaryReader reader)
        {
            int pCount = reader.ReadInt32();
            for (int i = 0; i < pCount; i++)
            {
                HexXY p = new HexXY(reader.ReadInt32(), reader.ReadInt32());
                int zCount = reader.ReadInt32();
                List<uint> zones = new List<uint>(zCount);
                for (int j = 0; j < zCount; j++)
                    zones.Add(reader.ReadUInt32());

                triggerZones.Add(p, zones);
            }
        }

        public virtual void LoadDynamicPart(System.IO.BinaryReader reader)
        {

        }

        public virtual void SaveStaticPart(System.IO.BinaryWriter writer)
        {
            writer.Write(triggerZones.Count);
            foreach (var p in triggerZones)
            {
                writer.Write(p.Key.x);
                writer.Write(p.Key.y);
                writer.Write(p.Value.Count);
                foreach (var z in p.Value)                
                    writer.Write(z);                   
                
            }
        }

        public virtual void SaveDynamicPart(System.IO.BinaryWriter writer)
        {

        }
    }
}
