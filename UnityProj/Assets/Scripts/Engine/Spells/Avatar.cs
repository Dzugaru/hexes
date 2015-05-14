using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public class Avatar
    {
        public SpellExecuting spell;

        public uint id;
        public HexXY pos;
        public uint dir;
        public Spell.CompiledRune rune;
        public float timeLeft;
        public FinishedState? error;
        public uint splitGroup;
        public IAvatarBehavior avatarBehavior;

        bool isArrowCrossDirLeft;

        public enum FinishedState
        {
            DiedCauseTooWeak,
            RuneIsNull
        }

        public Avatar(SpellExecuting spell, HexXY pos, uint dir, Spell.CompiledRune startRune, uint splitGroup, uint id)
        {
            this.spell = spell;            
            this.pos = pos;
            this.dir = dir;
            this.rune = startRune;
            this.timeLeft = 0;
            this.splitGroup = splitGroup;
            this.id = id;
        }

        public void Interpret()
        {
            if (rune == null)
            {
                error = FinishedState.RuneIsNull;
                return;
            }

            if (SpellExecuting.isLogging)            
                Logger.Log(id + "> interpret " + rune.type + " at " + rune.relPos);

            if (rune.type == RuneType.Compile ||
                IsAvatarBehaviorRune(rune.type))
            {
                InterpretSplit();
            }
            else if (IsArrowRune(rune.type))
            {
                InterpretArrow();
            }            
        }

        void InterpretSplit()
        {
            uint splitCount = 0;
            for(int i = 0; i < 6; i++)
            {
                var nrune = rune.neighs[i];

                if (nrune == null || nrune.type == RuneType.Compile) continue;
                if (IsPredicateRune(nrune.type)) continue;
                if (IsNumberRune(nrune.type)) continue;
                if (IsArrowRune(nrune.type) && !IsArrowFrom(nrune, (uint)i)) continue;

                if (IsAvatarBehaviorRune(nrune.type))
                {
                    var newAvatarBeh = CreateAvatarBehavior(nrune.type);
                    if (avatarBehavior == null)
                        avatarBehavior = newAvatarBeh;
                    else
                        spell.SpawnAvatar(nrune, nrune.relPos, spell.dir, avatarBehavior, true);
                }
                else
                {
                    if (splitCount == 0)
                    {
                        rune = nrune;
                    }
                    else
                    {
                        spell.SpawnAvatar(nrune, nrune.relPos, spell.dir, avatarBehavior, false);
                    }

                    ++splitCount;
                }
            }
        }

        void InterpretArrow()
        {
            uint fromDir = rune.dir;

            uint dirChange;
            switch (rune.type)
            {
                case RuneType.Arrow0: dirChange = 0; break;
                case RuneType.ArrowR60: dirChange = 1; break;
                case RuneType.ArrowR120: dirChange = 2; break;
                case RuneType.ArrowL120: dirChange = 4; break;
                case RuneType.ArrowL60: dirChange = 5; break;
                case RuneType.ArrowCross: dirChange = isArrowCrossDirLeft ? 5u : 0u; break;
                default: throw new Tools.AssertException();
            }

            uint toDir = (fromDir + dirChange) % 6;            

            rune = rune.neighs[toDir];          

            //Process cross
            if (rune != null && rune.type == RuneType.ArrowCross)
            {
                if (toDir == rune.dir)
                    isArrowCrossDirLeft = false;
                else if ((toDir + 1) % 6 == rune.dir)
                    isArrowCrossDirLeft = true;
                else
                    rune = null;
            }
        }

        static IAvatarBehavior CreateAvatarBehavior(RuneType type)
        {
            switch (type)
            {
                case RuneType.Flame: return new AvatarFlame(); 
                case RuneType.Wind: return new AvatarWind();
                case RuneType.Stone: return new AvatarStone();
            }

            throw new Tools.AssertException();
        }

        static bool IsArrowFrom(Spell.CompiledRune arrow, uint dir)
        {
            switch (arrow.type)
            {
                case RuneType.ArrowCross: return arrow.dir == dir || arrow.dir == (dir + 1) % 6;
                default: return arrow.dir == dir;
            }
        }

        static bool IsArrowRune(RuneType type)
        {
            switch (type)
            {
                case RuneType.Arrow0:
                case RuneType.ArrowCross:
                case RuneType.ArrowL120:
                case RuneType.ArrowL60:
                case RuneType.ArrowR120:
                case RuneType.ArrowR60:
                    return true;
                default:
                    return false;
            }
        }

        static bool IsPredicateRune(RuneType type)
        {
            switch (type)
            {
                case RuneType.PredicateAvatarRef:
                case RuneType.PredicateTileEmpty:
                case RuneType.PredicateTileMonster:
                case RuneType.PredicateTileWall:
                    return true;
                default:
                    return false;
            }
        }

        static bool IsNumberRune(RuneType type)
        {
            switch (type)
            {
                case RuneType.Number2:
                case RuneType.Number3:
                case RuneType.Number4:
                case RuneType.Number5:
                case RuneType.Number6:
                case RuneType.Number7:
                    return true;
                default:
                    return false;
            }
        }

        static bool IsAvatarBehaviorRune(RuneType type)
        {
            switch (type)
            {
                case RuneType.Flame:
                case RuneType.Wind:
                case RuneType.Stone:
                    return true;
                default:
                    return false;
            }
        }
    }
}
