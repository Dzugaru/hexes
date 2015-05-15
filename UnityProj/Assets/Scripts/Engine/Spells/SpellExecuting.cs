using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public class SpellExecuting
    {
        public const bool isLogging = true;

        public Entity caster;
        public Spell compiledSpell;
        public HexXY refPos;
        public uint dir;
        public List<Avatar> avatars = new List<Avatar>();
        public uint avatarLastID;
        public bool isExecuting;

        public Dictionary<uint, int> elementsUsedCounts = new Dictionary<uint, int>();

        public SpellExecuting(Entity caster, Spell compiledSpell, HexXY refPos, uint dir)
        {
            this.caster = caster;            
            this.compiledSpell = compiledSpell;
            this.refPos = refPos;
            this.dir = dir;
            this.isExecuting = true;
        }        

        public void SpawnAvatar(Spell.CompiledRune rune, Avatar from, HexXY pos, uint dir)
        {                    
            Avatar av = new Avatar(this, pos, dir, rune, avatarLastID++);
            if(from != null)
                from.avatarElement.CopyTo(av);            
            avatars.Add(av);

            if (isLogging)
                Logger.Log(av.id + " " + (av.avatarElement == null ? "[no element]" : av.avatarElement.GetType().Name) + " avatar spawned at " + pos);
        }

        public void Update(float dt)
        {
            for (int i = 0; i < avatars.Count; i++)
            {
                var av = avatars[i];
                av.timeLeft -= dt;
                if (av.timeLeft > 0) continue;

                if(av.finishState == null)
                    av.Interpret();

                if (av.finishState != null/* && av.finishState.Value != Avatar.FinishedState.FlowFinished*/)
                    avatars.RemoveAt(i--);

                //TODO:
                //if (av.finishState != null && av.finishState.Value == Avatar.FinishedState.FlowFinished)
                //    av.UpdateElement();
            }

            if (avatars.Count == 0)
                isExecuting = false;
        }

        public void UseElementRune(Spell.CompiledRune rune)
        {
            if (!elementsUsedCounts.ContainsKey(rune.listIdx))
                elementsUsedCounts[rune.listIdx] = 1;
            else
                ++elementsUsedCounts[rune.listIdx];

            //if (isLogging)
            //    Logger.Log("Used elemental rune at " + rune.relPos + ", count: " + elementsUsedCounts[rune.listIdx]);
        }
    }
}
