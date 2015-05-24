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

        public virtual void Update(float dt)
        {
            fibers.OnUpdate(dt);
        }

        public virtual void Start()
        {
            fibers = new Fibered();

            E.player = new Player();
            E.player.Spawn(PlayerSpawnPos);
        }

        protected T GetScriptEntity<T>(Enum idEnum) where T : Entity
        {
            return (T)Level.S.entityList.OfType<IHasScriptID>().FirstOrDefault(e => e.ID == Convert.ToInt32(idEnum));
        }

        protected IEnumerable<Entity> GetScriptEntities(Enum idEnum)
        {
            foreach (var scrEnt in Level.S.entityList.OfType<IHasScriptID>().Where(e => e.ID == Convert.ToInt32(idEnum)))
                yield return (Entity)scrEnt;
        }
    }
}
