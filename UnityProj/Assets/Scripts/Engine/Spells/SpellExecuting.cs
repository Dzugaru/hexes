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
        public Spell spell;
        public HexXY refPos;
        public uint dir;
        public List<Avatar> avatars = new List<Avatar>();
        public uint avatarLastSplitGroup, avatarLastID;
        public bool isExecuting;

        public SpellExecuting(Entity caster, Spell spell, HexXY refPos, uint dir)
        {
            this.caster = caster;            
            this.spell = spell;
            this.refPos = refPos;
            this.dir = dir;
            this.isExecuting = true;
        }        

        public void SpawnAvatar(Spell.CompiledRune rune, HexXY relPos, uint dir, IAvatarBehavior behavior, bool isNewSplitGroup)
        {
            if (isNewSplitGroup)
                ++avatarLastSplitGroup;            

            Avatar av = new Avatar(this, relPos + refPos, dir, rune, avatarLastSplitGroup, avatarLastID++);
            av.avatarBehavior = behavior;
            avatars.Add(av);

            if (isLogging)
                Logger.Log((avatarLastID - 1) + " avatar spawned at " + (relPos + refPos));
        }

        public void Update(float dt)
        {
            for (int i = 0; i < avatars.Count; i++)
            {
                var av = avatars[i];
                av.timeLeft -= dt;
                if (av.timeLeft > 0) continue;
                av.Interpret();
                if (av.error != null)
                    avatars.RemoveAt(i--);
            }

            if (avatars.Count == 0)
                isExecuting = false;
        }
    }
}
