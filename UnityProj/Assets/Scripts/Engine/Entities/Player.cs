using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Engine
{
    public class Player : Entity
    {
        public Spell currentSpell;
        Entity clickable;
                
        float speed;
        HexXY[] pathStorage;
        HexXY? dest;
        uint distToStop;
        HexXY interDest;
        bool isWalking;
        Vector2 fracPos, interFracDest;        
        const float blockForwardDist = 0.9f;
        const float blockBackDist = 0;
        HashSet<HexXY> blockedCells = new HashSet<HexXY>();
        List<HexXY> blockedCellsToDelete = new List<HexXY>();               

	    public Player() : base(EntityClass.Character, (uint)CharacterType.Player)
        {
            pathStorage = new HexXY[64];           
            speed = 2;
        }

        public override void Spawn(HexXY p)
        {
            base.Spawn(p);
            fracPos = pos.ToPlaneCoordinates();
            blockedCells.Add(pos);
            Level.S.SetPFBlockedMap(pos, WorldBlock.PFBlockType.DynamicBlocked);
            //G.S.DebugShowCell(pos);
        }

        public override void Update(float dt)
        {            
            if (!isWalking)
            {
                if (clickable != null && HexXY.Dist(clickable.pos, this.pos) <= 1)
                {
                    ((IClickable)clickable).Click();
                    clickable = null;
                }
            }
            else
            {
                Walking(dt);
            }

            base.Update(dt);
        }

        void Walking(float dt)
        {
            HexXY prevPos = pos;            

            float distToTargetSqr = (interFracDest - fracPos).sqrMagnitude;            

            Vector2 dir = (interFracDest - fracPos).normalized;

            //Calculate blocking
            //Debug.Log(distToTargetSqr + " " + (blockForwardDist * blockForwardDist));
            if (distToTargetSqr >= blockForwardDist * blockForwardDist)
            {
                Vector2 nextBlockPos = fracPos + dir * blockForwardDist;
                HexXY nextPosCell = HexXY.FromPlaneCoordinates(nextBlockPos);                
                if (!blockedCells.Contains(nextPosCell))
                {
                    if (Level.S.GetPFBlockedMap(nextPosCell) != WorldBlock.PFBlockType.Unblocked)
                    {
                        //Blocked by something - stop and move to nearest cell center
                        dest = interDest = pos;
                        //Debug.Log("Blocked dest " + pos);
                        interFracDest = pos.ToPlaneCoordinates();
                        float timeToGetThere = (fracPos - interFracDest).magnitude / (speed * 0.5f); //at half speed
                        Interfacing.PerformInterfaceMovePrecise(graphicsHandle, interFracDest, timeToGetThere);
                        return;
                    }
                    else
                    {
                        Level.S.SetPFBlockedMap(nextPosCell, WorldBlock.PFBlockType.DynamicBlocked);
                        //G.S.DebugShowCell(nextPosCell);
                        blockedCells.Add(nextPosCell);
                    }
                }
            }

            //Calculate movement
            Vector2 step = dir * speed * dt;
            if (step.sqrMagnitude == 0 || step.sqrMagnitude > distToTargetSqr)
            {
                fracPos = interFracDest;
                if (HexXY.Dist(interDest, dest.Value) <= distToStop)
                {
                    dest = null;
                    isWalking = false;
                    Interfacing.PerformInterfaceStop(graphicsHandle, pos);
                }
                else
                {
                    MovePart();
                }
            }
            else
            {
                fracPos += step;
            }  

            pos = HexXY.FromPlaneCoordinates(fracPos);

            if (pos != prevPos)
            {
                Level.S.RemoveEntity(prevPos, this);                
                Level.S.AddEntity(pos, this);

                //Remove blocked cells that are behind us
                blockedCellsToDelete.Clear();

                foreach (var bc in blockedCells)
                {
                    Vector2 bcFrac = bc.ToPlaneCoordinates();
                    if (bc != pos && Vector2.Dot(fracPos - bcFrac, dir) > blockBackDist)                    
                        blockedCellsToDelete.Add(bc);                    
                }

                foreach (var dbc in blockedCellsToDelete)
                {
                    blockedCells.Remove(dbc);
                    Level.S.SetPFBlockedMap(dbc, WorldBlock.PFBlockType.Unblocked);
                    //G.S.DebugHideCell(dbc);
                }                
            }
        }

        //Trace first straight line dest and move there
        void MovePart()
        {
            //Debug.Log("MOVEPART " + p);
            uint? pathLen = Pathfinder.FindPath(pos, dest.Value, pathStorage, distToStop, 10);
            if (pathLen.HasValue && pathLen.Value > 0)
            {
                HexXY firstPathCell = pathStorage[0];               
                if (!blockedCells.Contains(firstPathCell) && Level.S.GetPFBlockedMap(firstPathCell) != WorldBlock.PFBlockType.Unblocked)
                {
                    //Even first cell is blocked, just rotate there
                    dest = null;
                    isWalking = false;
                    Interfacing.PerformInterfaceStop(graphicsHandle, pos);
                    Interfacing.PerformInterfaceUpdateRotation(graphicsHandle, (uint)HexXY.neighbours.IndexOf(pathStorage[0] - pos));
                }
                else
                {
                    //Try to trace a line to these
                    //First line that will be blocked will determine max straight-line destination
                    //Path to first cell should not be blocked so start with second    
                    if (pathLen.Value == 1)
                    {
                        interDest = pathStorage[0];
                        interFracDest = interDest.ToPlaneCoordinates();
                    }
                    else
                    {
                        for (int i = 1; i < pathLen.Value; i++)
                        {
                            HexXY inter = pathStorage[i];

                            //Check if straight line backwards is blocked 
                            Vector2 linePos = inter.ToPlaneCoordinates();
                            Vector2 lineDir = (fracPos - linePos).normalized;
                            bool isLineBlocked = false;
                            HexXY traced = inter;

                            do
                            {
                                float minDist = float.MaxValue;
                                float lineShift = 0;
                                HexXY nextTraced = new HexXY(0, 0);
                                for (int n = 0; n < 6; n++)
                                {
                                    HexXY np = traced + HexXY.neighbours[n];
                                    Vector2 v = np.ToPlaneCoordinates() - linePos;
                                    float lineLen = Vector2.Dot(v, lineDir);
                                    if (lineLen < 0) continue;
                                    float dist = (lineDir * lineLen - v).magnitude;
                                    if (dist < 0.8f) //Line intersects cell np (circle approximated, not hexagon though, and larger (should be less than cos(60)) so it will not cut walls)
                                    {
                                        if (!blockedCells.Contains(np) && Level.S.GetPFBlockedMap(np) != WorldBlock.PFBlockType.Unblocked)
                                        {
                                            isLineBlocked = true;
                                            break;
                                        }

                                        if (dist < minDist)
                                        {
                                            minDist = dist;
                                            nextTraced = np;
                                            lineShift = lineLen;
                                        }
                                    }
                                }

                                traced = nextTraced;
                                linePos += lineShift * lineDir;
                            } while (!isLineBlocked && traced != pos);

                            if (isLineBlocked) //Choose the previous cell
                            {
                                interDest = pathStorage[i - 1];
                                interFracDest = interDest.ToPlaneCoordinates();
                                break;
                            }

                            //Got to the last cell - use it
                            if (i == pathLen.Value - 1)
                            {
                                interDest = inter;
                                interFracDest = interDest.ToPlaneCoordinates();
                            }
                        }
                    }

                    float timeToGetThere = (fracPos - interFracDest).magnitude / speed;
                    Interfacing.PerformInterfaceMovePrecise(graphicsHandle, interFracDest, timeToGetThere);
                    isWalking = true;
                }
            }
            else
            {
                dest = null;
                isWalking = false;
                Interfacing.PerformInterfaceStop(graphicsHandle, pos);
            }
        }

        public void Move(HexXY p)
        {
            if (Level.S.GetPFBlockedMap(p) != WorldBlock.PFBlockType.StaticBlocked &&
               (!dest.HasValue || p != dest))
            {
                clickable = null;
                dest = p;
                distToStop = 0;
                MovePart();                
            }            
        }

        public bool DrawRune(RuneType type, HexXY p)
        {
            bool isSuccess = !isWalking && Rune.CanDraw(this, type, p);
            if (isSuccess) Rune.DrawRune(this, type, p);
            return isSuccess;             
        }

        public bool EraseRune(HexXY p)
        {
            bool isSuccess = !isWalking && Rune.CanErase(this, p);
            if (isSuccess) Rune.EraseRune(p);
            return isSuccess;            
        }

        public bool CompileSpell(HexXY p)
        {
            Rune compileRune =                                
                (Rune)Level.S.GetEntities(p).FirstOrDefault(e => e is Rune && Avatar.IsAvatarElementRune((RuneType)e.entityType));

            bool isSuccess = compileRune != null;
            if (isSuccess) currentSpell = Spell.CompileSpell(compileRune, p);            
            return isSuccess;
        }

        public bool CastCurrentSpell(HexXY p)
        {
            bool isSuccess = currentSpell != null && HexXY.Dist(pos, p) == 1;
            if (isSuccess) currentSpell.Cast(this, (uint)HexXY.neighbours.IndexOf(p - pos));
            return isSuccess;            
        }

      

        public void Clicked(Entity clickable)
        {
            if (HexXY.Dist(clickable.pos, this.pos) <= 1)
            {
                ((IClickable)clickable).Click();
            }
            else
            {
                this.clickable = clickable;
                dest = clickable.pos;
                distToStop = 1;
                MovePart();
            }
        }
    } 
}


