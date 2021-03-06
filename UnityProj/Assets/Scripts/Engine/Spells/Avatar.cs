﻿using System;
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
        public uint flowDir;
        public float timeLeft;
        public FinishedState? finishState;        
        public IAvatarElement avatarElement;

        bool needsFlow;
        bool isArrowCrossDirLeft;

        uint repeatCounter;
        Dictionary<HexXY, uint> loopCounters = new Dictionary<HexXY, uint>();

        Queue<Spell.CompiledRune> blobFront = new Queue<Spell.CompiledRune>();
        HashSet<Spell.CompiledRune> blobAll = new HashSet<Spell.CompiledRune>(new RunePosEqualityComparer());

        HashSet<HexXY> arrowsProcessed = new HashSet<HexXY>(); //This is for flow arrow cycle detection
        List<Spell.CompiledRune> additionalInterpretedRunes = new List<Spell.CompiledRune>();

        public enum FinishedState
        {
            FlowFinished,
            NoManaLeft,            
            PredicateParseError,
            CantMoveThere,
            CantFork                         
        }

        public Avatar(SpellExecuting spell, HexXY pos, uint dir, Spell.CompiledRune startRune, uint id)
        {
            this.spell = spell;            
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
                finishState = FinishedState.FlowFinished;
                return;
            }

            if (SpellExecuting.isLogging)
                Logger.Log(id + " " + (avatarElement == null ? "[no element]" : avatarElement.GetType().Name) + "> interpret " + rune.type + " at " + rune.relPos);
           

            //This is written up here and not in the end for correct fork timing
            if (needsFlow && repeatCounter == 0)
                InterpretFlow();

            if (finishState != null)
                return;

            additionalInterpretedRunes.Clear();

            if (IsRepeatableByNumberRune(rune.type))
            {
                //Actually no need to reinterpret numbers, if repeatCounter > 0
                //but its needed for proper additionalInterpretedRunes highlight
                uint repCount = InterpretNearNumber();
                
                if (repeatCounter > 0)
                    --repeatCounter;
                else
                {                    
                    if (repCount > 0)
                        repeatCounter = repCount - 1;
                }                
            }                     

            var currRune = rune;                  

            needsFlow = true;            
            if (IsArrowRune(rune.type))
            {
                rune = InterpretArrowSeq(rune, true, true);
                needsFlow = false;
            }
            else if (IsAvatarElementRune(rune.type))
            {
                InterpretChangeElement();
            }
            else if (IsMovementCommandRune(rune.type))
            {
                InterpretMovementCommand();
            }
            else if (rune.type == RuneType.If)
            {
                InterpretIf();
                needsFlow = false;
            }
            else
            {
                finishState = FinishedState.FlowFinished;
            }

            if (avatarElement != null)
                timeLeft += avatarElement.OnInterpret(currRune, additionalInterpretedRunes);
        }    

        void InterpretFlow()
        {
            uint forkCount = 0;
            Spell.CompiledRune nextRune = null;
            for (int i = 0; i < 6; i++)
            {
                var nrune = rune.neighs[i];
                if (nrune == null) continue;

                bool isFlowArrow = IsArrowRune(nrune.type) || nrune.type == RuneType.If;
                if ((isFlowArrow && !IsArrowFrom(nrune, (uint)i)) || (!isFlowArrow && i != flowDir)) continue;
                if (i == flowDir && !IsFlowCorrect(rune, flowDir)) continue;

                if (forkCount == 0)
                    nextRune = nrune;
                else
                {
                    if (spell.caster.Mana >= avatarElement.ForkManaCost && avatarElement.CanAvatarFork())
                    {
                        spell.SpawnAvatar(nrune, this, pos, dir);
                    }
                    else
                    {
                        finishState = FinishedState.CantFork;
                        break;
                    }
                }

                ++forkCount;
            }

            if (forkCount == 0) finishState = FinishedState.FlowFinished;
            else rune = nextRune;
        }

        Spell.CompiledRune InterpretArrowSeq(Spell.CompiledRune r, bool onlyOneRune, bool setFlowDir)
        {
            do
            {
                if (!onlyOneRune)
                {
                    additionalInterpretedRunes.Add(r);
                }

                if (arrowsProcessed.Contains(r.relPos))
                {
                    arrowsProcessed.Clear();
                    return null; //Cycle detected
                }
                arrowsProcessed.Add(r.relPos);

                uint fromDir = r.dir;

                uint dirChange;
                switch (r.type)
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
                if (!IsFlowCorrect(r, toDir))
                {
                    arrowsProcessed.Clear();
                    return null;
                }
                r = r.neighs[toDir];
                if (r == null)
                {
                    arrowsProcessed.Clear();
                    return null;
                }
                if (!IsArrowRune(r.type))
                {
                    arrowsProcessed.Clear();
                    //if (setFlowDir)                    
                    //    flowDir = toDir;                    
                    return r;
                }
            } while (!onlyOneRune);

            return r;
        }

        bool IsFlowCorrect(Spell.CompiledRune from, uint toDir)
        {
            var to = from.neighs[toDir];

            if (!IsFlowInterpretableRune(to.type)) return false;

            //Logger.Log(from.type + " " + to.type + " " + toDir);

            //Cross arrow is special
            if (to != null && to.type == RuneType.ArrowCross)
            {   
                if (toDir == to.dir)
                    isArrowCrossDirLeft = false;
                else if (toDir == (to.dir + 5) % 6)
                    isArrowCrossDirLeft = true;
                else
                    return false;
            }

            //TODO: cant enter "if" rune from two of its directions too...

            return true;
        }

        void InterpretChangeElement()
        {            
            IAvatarElement newEl;

            switch (rune.type)
            {
                case RuneType.Flame: newEl = new AvatarFlame(this, rune.listIdx, 0.1f); break;
                case RuneType.Wind: newEl = new AvatarLearn(this, rune.listIdx, 0.3f); break;
                case RuneType.Stone: newEl = new AvatarStone(this, rune.listIdx); break;
                default: throw new Tools.AssertException();
            }

            if (spell.caster.Mana < newEl.ForkManaCost)
            {
                finishState = FinishedState.NoManaLeft;
                return;
            }
            else
            {
                spell.caster.SpendMana(newEl.ForkManaCost);
            }

            if (avatarElement == null || newEl.GetType() != avatarElement.GetType()) //TODO: what if it's the same?
            {
                if (avatarElement != null)
                {
                    avatarElement.OnDie();
                }               

                avatarElement = newEl;
                if (avatarElement is Entity)
                    ((Entity)avatarElement).dir = dir;
                avatarElement.OnSpawn();
            }

            if (SpellExecuting.isLogging)
                Logger.Log(id + " " + (avatarElement == null ? "[no element]" : avatarElement.GetType().Name) + "> change element to " + rune.type);
        }

        void InterpretMovementCommand()
        {
            //TODO: use elemental rune in spell if drawing
            //TODO: search number nearby

            HexXY dpos = new HexXY(0, 0);
            bool isDraw = false;

            switch (rune.type)
            {
                case RuneType.AvatarForward:
                case RuneType.AvatarForwardDraw:
                case RuneType.AvatarForwardDupDraw:
                    dpos = HexXY.neighbours[dir];
                    isDraw = rune.type == RuneType.AvatarForwardDraw;
                    break;

                case RuneType.AvatarLeft:
                    dir = (dir + 5) % 6;
                    break;

                case RuneType.AvatarRight:
                    dir = (dir + 1) % 6;
                    break;

                case RuneType.AvatarWalkDir:
                case RuneType.AvatarWalkDirDraw:
                    dpos = HexXY.neighbours[(spell.dir + rune.dir) % 6];
                    isDraw = rune.type == RuneType.AvatarWalkDirDraw;
                    break;
            }

            if (dpos != new HexXY(0, 0))
            {
                HexXY newPos = pos + dpos;
                //if (!WorldBlock.S.pfIsPassable(newPos))
                //{
                //    finishState = FinishedState.CantMoveThere;
                //    //Do nothing
                //}
                //else
                //{
                avatarElement.OnMove(pos, newPos, isDraw);
                pos = newPos;

                if (SpellExecuting.isLogging)
                    Logger.Log(id + " " + (avatarElement == null ? "[no element]" : avatarElement.GetType().Name) + "> moved to " + pos);
                //}
            }
            else
            {
                avatarElement.OnRotate(dir);
            }
        }

        class RunePosEqualityComparer : IEqualityComparer<Spell.CompiledRune>
        {
            public bool Equals(Spell.CompiledRune a, Spell.CompiledRune b)
            {
                return a.relPos == b.relPos;
            }

            public int GetHashCode(Spell.CompiledRune a)
            {
                return a.relPos.GetHashCode();
            }
        }

        bool InterpretPredicate(Spell.CompiledRune predRune)
        {
            //Compute connected component and find avatar ref position
            Spell.CompiledRune avatarRefRune = null;            

            blobFront.Enqueue(predRune);
            blobAll.Add(predRune);

            do
            {
                var c = blobFront.Dequeue();
                additionalInterpretedRunes.Add(c);
                if (c.type == RuneType.PredicateAvatarRef)
                {
                    if (avatarRefRune != null)
                    {
                        //Double avatar ref error
                        finishState = FinishedState.PredicateParseError;
                        return false;
                    }
                    else
                    {
                        avatarRefRune = c;
                    }
                }

                foreach (var n in c.neighs)
                {
                    if (n == null || !IsPredicateRune(n.type) || blobAll.Contains(n)) continue;
                    blobFront.Enqueue(n);
                    blobAll.Add(n);
                }
            } while (blobFront.Count > 0);

            HexXY predRefPos;
            if (avatarRefRune != null)
            {
                predRefPos = avatarRefRune.relPos;
            }
            else
            {
                if (blobAll.Count > 1)
                {
                    //No avatar reference in predicate with more than one rune
                    finishState = FinishedState.PredicateParseError;
                    return false;
                }
                else
                {
                    predRefPos = blobAll.First().relPos;
                }
            }

            //Check predicate
            bool isMatch = true;
            foreach (var prune in blobAll)
            {
                HexXY checkPos = prune.relPos - predRefPos;
                for (int i = 0; i < dir; i++)
                    checkPos = checkPos.RotateRight(new HexXY(0, 0));
                checkPos += this.pos;

                switch (prune.type)
                {
                    case RuneType.PredicateTileEmpty:
                        {
                            var blType = Level.S.GetPFBlockedMap(checkPos);
                            if (blType != WorldBlock.PFBlockType.Unblocked &&
                                blType != WorldBlock.PFBlockType.DynamicBlocked)
                                isMatch = false;
                            break;
                        }
                    case RuneType.PredicateTileWall:
                        if (Level.S.GetPFBlockedMap(checkPos) != WorldBlock.PFBlockType.EdgeBlocked &&
                            Level.S.GetPFBlockedMap(checkPos) != WorldBlock.PFBlockType.StaticBlocked &&
                            Level.S.GetPFBlockedMap(checkPos) != WorldBlock.PFBlockType.DoorBlocked)
                            isMatch = false;
                        break;
                    case RuneType.PredicateTileMonster:
                        if (!Level.S.GetEntities(checkPos).Any(e => e is Mob))
                            isMatch = false;
                        break;
                }
                if (!isMatch) break;                
            }

            blobAll.Clear();

            if (SpellExecuting.isLogging)
                Logger.Log("predicate at " + predRune.relPos + " is " + isMatch);

            return isMatch;
        }

        void InterpretIf()
        {
            bool isTrue = false;

            //Checking only separate patterns
            //TODO: what if pattern is attached like this: (p) (if) (p) where (p)'s are connected elsewhere?
            //it will be evaluated twice!
            bool isPredAtZero = false, isPredAtPrev = false;

            for (int i = 0; i < 6; i++)
            {
                var nrune = rune.neighs[i];
                if (i != rune.dir && i != (rune.dir + 5) % 6 && nrune != null && IsArrowRune(nrune.type) && IsArrowFrom(nrune, (uint)i))
                {
                    //Find following arrows
                    nrune = InterpretArrowSeq(nrune, false, false);
                    if (nrune == null || !IsPredicateRune(nrune.type)) continue;

                    isTrue = isTrue || InterpretPredicate(nrune); //using OR
                    isPredAtPrev = false;
                }
                else
                {                   
                    if (nrune == null || !IsPredicateRune(nrune.type))
                    {
                        isPredAtPrev = false;
                        continue;
                    }
                    if (isPredAtPrev || (i == 5 && isPredAtZero)) continue;

                    isTrue = isTrue || InterpretPredicate(nrune); //using OR

                    if (i == 0)
                        isPredAtZero = true;
                    isPredAtPrev = true;
                }               
            }

            uint toDir;

            if (isTrue)
                toDir = rune.dir;
            else
                toDir = (rune.dir + 5) % 6;

            //flowDir = toDir;

            if (!IsFlowCorrect(rune, toDir)) rune = null;
            else rune = rune.neighs[toDir];            
        }

        uint InterpretNearNumber()
        {
            uint count = 0;

            blobFront.Enqueue(rune);
            blobAll.Add(rune);

            do
            {
                var c = blobFront.Dequeue();

                foreach (var n in c.neighs)
                {                    
                    if (n == null || !IsNumberRune(n.type) || blobAll.Contains(n)) continue;
                    additionalInterpretedRunes.Add(n);
                    blobFront.Enqueue(n);
                    blobAll.Add(n);

                    switch (n.type)
                    {
                        case RuneType.Number2: count += 2; break;
                        case RuneType.Number3: count += 3; break;
                        case RuneType.Number4: count += 4; break;
                        case RuneType.Number5: count += 5; break;
                        case RuneType.Number6: count += 6; break;
                        case RuneType.Number7: count += 7; break;
                    }                    
                }
            } while (blobFront.Count > 0);

            blobAll.Clear();

            return count;
        }
           

        static bool IsArrowFrom(Spell.CompiledRune arrow, uint dir)
        {
            switch (arrow.type)
            {
                case RuneType.ArrowCross: return arrow.dir == dir || arrow.dir == (dir + 1) % 6;                
                default: return arrow.dir == dir;
            }
        }

        static bool IsRepeatableByNumberRune(RuneType type)
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

        public static bool IsArrowRune(RuneType type)
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

        public static bool IsPredicateRune(RuneType type)
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

        public static bool IsNumberRune(RuneType type)
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

        public static bool IsMovementCommandRune(RuneType type)
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

        public static bool IsFlowInterpretableRune(RuneType type)
        {
            return !IsNumberRune(type) && !IsPredicateRune(type);
        }

       
    }
}
