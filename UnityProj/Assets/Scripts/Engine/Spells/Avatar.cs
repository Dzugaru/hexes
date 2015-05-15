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
        public FinishedState? finishState;
        public uint avatarElementRuneIdx;
        public IAvatarElement avatarElement;

        bool isArrowCrossDirLeft;

        public enum FinishedState
        {
            FlowFinished,
            DiedCauseTooWeak,
            RuneIsNull
        }

        public Avatar(SpellExecuting spell, IAvatarElement element, HexXY pos, uint dir, Spell.CompiledRune startRune, uint id)
        {
            this.spell = spell;
            this.avatarElement = element;
            this.pos = pos;
            this.dir = dir;
            this.rune = startRune;
            this.timeLeft = 0;            
            this.id = id;
        }

        public void Interpret()
        {
            if (rune == null)
            {
                finishState = FinishedState.RuneIsNull;
                return;
            }

            if (SpellExecuting.isLogging)            
                Logger.Log(id + " " + (avatarElement == null ? "[no element]" : avatarElement.GetType().Name) + "> interpret " + rune.type + " at " + rune.relPos);

            bool isArrow = IsArrowRune(rune.type);

            if (isArrow)
            {
                InterpretArrow();
            }
            else if (IsAvatarElementRune(rune.type))
            {
                InterpretChangeElement();
            }
            else if (IsMovementCommandRune(rune.type))
            {
                InterpretMovementCommand();
            }      
            
            if(!isArrow)
                InterpretFlow();
        }

        void InterpretChangeElement()
        {
            avatarElementRuneIdx = rune.listIdx;
            avatarElement = CreateAvatarElement(rune.type); 
            spell.UseElementRune(rune);

            if (SpellExecuting.isLogging)
                Logger.Log(id + " " + (avatarElement == null ? "[no element]" : avatarElement.GetType().Name) + "> change element to " + rune.type);
        }

        void InterpretMovementCommand()
        {
            //TODO: use elemental rune in spell if drawing
            switch (rune.type)
            {
                case RuneType.AvatarForward:
                case RuneType.AvatarForwardDraw:
                case RuneType.AvatarForwardDupDraw:
                    pos += HexXY.neighbours[(spell.dir + dir) % 6];
                    break;

                case RuneType.AvatarLeft:
                    dir = (dir + 5) % 6;
                    break;

                case RuneType.AvatarRight:
                    dir = (dir + 1) % 6;
                    break;

                case RuneType.AvatarWalkDir:
                case RuneType.AvatarWalkDirDraw:
                    pos += HexXY.neighbours[(spell.dir + rune.dir) % 6];
                    break;
            }

            if (SpellExecuting.isLogging)
                Logger.Log(id + " " + (avatarElement == null ? "[no element]" : avatarElement.GetType().Name) + "> moved to " + pos);
        }

        void InterpretFlow()
        {
            uint forkCount = 0;
            Spell.CompiledRune nextRune = null;
            for(int i = 0; i < 6; i++)
            {
                var nrune = rune.neighs[i];
                if (nrune == null) continue;
                

                bool isArrow = IsArrowRune(nrune.type);
                if (isArrow && !IsArrowFrom(nrune, (uint)i) || !isArrow && i != 0) continue;

                if (forkCount == 0)
                    nextRune = nrune;
                else
                {
                    spell.SpawnAvatar(nrune, avatarElement.Clone(), pos, dir);
                    spell.UseElementRune(spell.compiledSpell.allRunes[(int)avatarElementRuneIdx]);
                }

                ++forkCount;               
            }

            if (forkCount == 0) finishState = FinishedState.FlowFinished;
            else rune = nextRune;
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

            //Cross arrow is special
            if (rune != null && rune.type == RuneType.ArrowCross)
            {
                if (toDir == rune.dir)
                    isArrowCrossDirLeft = false;
                else if ((toDir + 1) % 6 == rune.dir)
                    isArrowCrossDirLeft = true;
                else
                    rune = null;
            }

            //TODO: cant enter if rune from two of its directions too...
        }

        static IAvatarElement CreateAvatarElement(RuneType type)
        {
            switch (type)
            {
                case RuneType.Flame: return new AvatarFlame(); 
                case RuneType.Wind: return new AvatarWind();
                case RuneType.Stone: return new AvatarStone();
            }

            throw new Tools.AssertException();
        }

        public void UpdateElement()
        {
            //TODO
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

        static bool IsMovementCommandRune(RuneType type)
        {
            switch (type)
            {
                case RuneType.AvatarForward:
                case RuneType.AvatarForwardDraw:
                case RuneType.AvatarForwardDupDraw:
                case RuneType.AvatarLeft:
                case RuneType.AvatarRight:
                case RuneType.AvatarWalkDir:
                case RuneType.AvatarWalkDirDraw:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsAvatarElementRune(RuneType type)
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
