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

        public Entity caster;
        public Spell compiledSpell;
        public HexXY refPos;
        public uint dir;
        public List<Avatar> avatars = new List<Avatar>();
        public uint avatarLastID;
        public bool isExecuting;

        Dictionary<uint, float> elementalPower = new Dictionary<uint, float>();

        public SpellExecuting(Entity caster, Spell compiledSpell, HexXY refPos, uint dir)
        {
            this.caster = caster;            
            this.compiledSpell = compiledSpell;
            this.refPos = refPos;
            this.dir = dir;
            this.isExecuting = true;
        }        

        public void SpawnAvatar(Spell.CompiledRune rune, Avatar forkFrom, HexXY pos, uint dir)
        {
            Avatar av = new Avatar(this, pos, dir, rune, avatarLastID++);            
            if (forkFrom != null)
            {
                av.timeLeft = forkFrom.timeLeft;
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
                    Logger.Log(av.finishState.ToString());
                    avatars.RemoveAt(i--);
                    av.avatarElement.OnDie();
                }

                if (av.finishState == null)
                    av.Interpret();
            }

            if (avatars.Count == 0)
                isExecuting = false;
        }

        public float GetElementalPower(uint idx)
        {
            float pow;
            if (!elementalPower.TryGetValue(idx, out pow)) return 1;
            else return pow;
        }

        public void UseElementalPower(uint idx, float amount)
        {
            if (!elementalPower.ContainsKey(idx))
                elementalPower[idx] = Mathf.Max(0, 1 - amount);
            else
                elementalPower[idx] = Mathf.Max(0, elementalPower[idx] - amount);

            //Debug.Log("power used: " + amount + ", left: " + elementalPower[idx]);
        }

        public void Die()
        {
            isExecuting = false;
            foreach (var av in avatars)
            {
                if(av.avatarElement != null)
                    av.avatarElement.OnDie();
            }
        }
    }
}
