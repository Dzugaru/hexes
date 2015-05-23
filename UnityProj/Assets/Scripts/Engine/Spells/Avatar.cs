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
        public uint flowDir;
        public float timeLeft;
        public FinishedState? finishState;        
        public IAvatarElement avatarElement;

        bool isArrowCrossDirLeft;
        HashSet<HexXY> arrowsProcessed = new HashSet<HexXY>(); //This is for flow arrow cycle detection

        public enum FinishedState
        {
            FlowFinished,
            DiedCauseTooWeak,            
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
            
            var currRune = rune;                       

            bool needsFlow = true;
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
                timeLeft += avatarElement.OnInterpret(currRune);

            if (needsFlow && finishState == null)
                InterpretFlow();
        }    

        void InterpretFlow()
        {
            uint forkCount = 0;
            Spell.CompiledRune nextRune = null;
            for (int i = 0; i < 6; i++)
            {
                var nrune = rune.neighs[i];
                if (nrune == null) continue;

                bool isArrow = IsArrowRune(nrune.type);
                if ((isArrow && !IsArrowFrom(nrune, (uint)i)) || (!isArrow && i != flowDir)) continue;
                if (i == flowDir && !IsFlowCorrect(rune, flowDir)) continue;

                if (forkCount == 0)
                    nextRune = nrune;
                else
                {
                    if (avatarElement.CanAvatarFork())
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
                    if (setFlowDir)                    
                        flowDir = toDir;                    
                    return r;
                }
            } while (!onlyOneRune);

            return r;
        }

        bool IsFlowCorrect(Spell.CompiledRune from, uint toDir)
        {
            from = from.neighs[toDir];

            //Cross arrow is special
            if (rune != null && rune.type == RuneType.ArrowCross)
            {
                if (toDir == rune.dir)
                    isArrowCrossDirLeft = false;
                else if ((toDir + 1) % 6 == rune.dir)
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
                case RuneType.Flame: newEl = new AvatarFlame(this, rune.listIdx); break;
                case RuneType.Wind: newEl = new AvatarLearn(this, rune.listIdx, 3); break;
                case RuneType.Stone: newEl = new AvatarStone(this, rune.listIdx); break;
                default: throw new Tools.AssertException();
            }

            if (avatarElement == null || newEl.GetType() != avatarElement.GetType()) //TODO: what if it's the same?
            {
                if (avatarElement != null)
                {
                    avatarElement.OnDie();
                }

                avatarElement = newEl;
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

        class InterpretPredicateRuneEqualityComparer : IEqualityComparer<Spell.CompiledRune>
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

            Queue<Spell.CompiledRune> front = new Queue<Spell.CompiledRune>();
            HashSet<Spell.CompiledRune> all = new HashSet<Spell.CompiledRune>(new InterpretPredicateRuneEqualityComparer());

            front.Enqueue(predRune);
            all.Add(predRune);

            do
            {
                var c = front.Dequeue();
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
                    if (n == null || !IsPredicateRune(n.type) || all.Contains(n)) continue;
                    front.Enqueue(n);
                    all.Add(n);
                }
            } while (front.Count > 0);

            HexXY predRefPos;
            if (avatarRefRune != null)
            {
                predRefPos = avatarRefRune.relPos;
            }
            else
            {
                if (all.Count > 1)
                {
                    //No avatar reference in predicate with more than one rune
                    finishState = FinishedState.PredicateParseError;
                    return false;
                }
                else
                {
                    predRefPos = all.First().relPos;
                }
            }

            //Check predicate
            bool isMatch = true;
            foreach (var prune in all)
            {
                HexXY checkPos = prune.relPos - predRefPos;
                for (int i = 0; i < dir; i++)
                    checkPos = checkPos.RotateRight(new HexXY(0, 0));
                checkPos += this.pos;

                switch (prune.type)
                {
                    case RuneType.PredicateTileEmpty:
                        if (Level.S.GetPFBlockedMap(checkPos) != WorldBlock.PFBlockType.StaticBlocked)
                            isMatch = false;
                        break;
                    case RuneType.PredicateTileWall:
                        if (Level.S.GetPFBlockedMap(checkPos) == WorldBlock.PFBlockType.StaticBlocked)
                            isMatch = false;
                        break;
                    case RuneType.PredicateTileMonster:
                        if (!Level.S.GetEntities(checkPos).Any(e => e is Mob))
                            isMatch = false;
                        break;
                }
                if (!isMatch) break;                
            }

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
                if (i != 0 && i != 5 && nrune != null && IsArrowRune(nrune.type) && IsArrowFrom(nrune, (uint)i))
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

            if (!IsFlowCorrect(rune, toDir)) rune = null;
            else rune = rune.neighs[toDir];            
        }
           

        static bool IsArrowFrom(Spell.CompiledRune arrow, uint dir)
        {
            switch (arrow.type)
            {
                case RuneType.ArrowCross: return arrow.dir == dir || arrow.dir == (dir + 1) % 6;
                default: return arrow.dir == dir;
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
