using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Engine
{
    public class SpellExecuting
    {
        public const bool isLogging = false;

        public ICaster caster;
        public Spell compiledSpell;
        public HexXY refPos;
        public uint dir;
        public List<Avatar> avatars = new List<Avatar>();
        public uint avatarLastID;
        public bool isExecuting;
        public bool isRealRunesLocked;      

        public SpellExecuting(ICaster caster, Spell compiledSpell, HexXY refPos, uint dir, bool lockRealRunes)
        {
            this.caster = caster;            
            this.compiledSpell = compiledSpell;
            this.refPos = refPos;
            this.dir = dir;
            this.isExecuting = true;
            this.isRealRunesLocked = lockRealRunes;

            if (lockRealRunes)            
                foreach (var rune in compiledSpell.allRunes)
                    compiledSpell.GetRealRune(rune).isUsedInSpell = true;            
        }        

        public void SpawnAvatar(Spell.CompiledRune rune, Avatar forkFrom, HexXY pos, uint dir)
        {
            Avatar av = new Avatar(this, pos, dir, rune, avatarLastID++);
            if (forkFrom != null)
            {
                av.timeLeft = forkFrom.timeLeft;
                caster.SpendMana(forkFrom.avatarElement.ForkManaCost);
                forkFrom.avatarElement.ForkTo(av);
                if (av.avatarElement is Entity)
                    ((Entity)av.avatarElement).dir = av.dir;
                av.avatarElement.OnSpawn();
            }            

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

                if (av.finishState != null)
                {
                    //Logger.Log(av.finishState.ToString());
                    avatars.RemoveAt(i--);
                    if(av.avatarElement != null)
                        av.avatarElement.OnDie();
                }

                if (av.finishState == null)
                    av.Interpret();
            }

            if (avatars.Count == 0)
                isExecuting = false;
        }       

        public void Die()
        {
            isExecuting = false;
            foreach (var av in avatars)
            {
                if(av.avatarElement != null)
                    av.avatarElement.OnDie();
            }

            if (isRealRunesLocked)
                foreach (var rune in compiledSpell.allRunes)
                    compiledSpell.GetRealRune(rune).isUsedInSpell = false;
        }
    }
}
