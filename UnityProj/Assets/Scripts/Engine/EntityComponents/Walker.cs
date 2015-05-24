using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public interface IWalker { }

    public class Walker : IEntityComponent
    {
        Entity entity;

        HexXY[] pathStorage;
        uint pathStart, pathEnd;
        HexXY prevTile, pfBlockedTile;
        public HexXY? dest;
        uint distToStop, blockedCost;
        bool onTileCenter;
        float speed, invSpeed, distToNextTile;
        public bool isWalkBlocked, isWalking;
        public float walkBlockedTime;
        bool shouldRecalcPath;        

        public Walker(Entity ent, int maxPathLen)
        {
            this.entity = ent;
            pathStorage = new HexXY[maxPathLen];
        }

        public void OnSpawn(HexXY pos)
        {
            pathStart = pathEnd = 0;
            prevTile = pfBlockedTile = pos;
            dest = null;
            distToNextTile = 1;
            onTileCenter = true;
            isWalkBlocked = isWalking = false;
            walkBlockedTime = 0;
            shouldRecalcPath = false;
            Level.S.SetPFBlockedMap(pos, WorldBlock.PFBlockType.DynamicBlocked);            
        }

        public void OnDie()
        {
            Level.S.SetPFBlockedMap(pfBlockedTile, WorldBlock.PFBlockType.Unblocked);
        }

        public bool OnUpdate(float dt)
        {
            if (onTileCenter && shouldRecalcPath)
            {   
                var pathLen = Pathfinder.FindPath(entity.pos, dest.Value, pathStorage, distToStop, blockedCost);
                //Logger.Log(pathLen.ToString());
                pathStart = 0;
                shouldRecalcPath = false;
                isWalkBlocked = false;

                if (!pathLen.HasValue || pathLen.Value == 0)
                {
                    pathEnd = 0;
                    if (isWalking)
                    {                        
                        Interfacing.PerformInterfaceStop(entity.graphicsHandle, entity.pos);
                        isWalking = false;
                    }
                    return true;
                }

                
                pathEnd = pathLen.Value;                
            }

            if (pathStart == pathEnd) return true;
            var nextTile = pathStorage[pathStart];

            if (distToNextTile > 0.5)
                entity.pos = prevTile;
            else
                entity.pos = nextTile;

            if (onTileCenter)
            {
                if (Level.S.GetPFBlockedMap(nextTile) == WorldBlock.PFBlockType.DynamicBlocked)
                {
                    if (!isWalkBlocked)
                    {
                        isWalkBlocked = true;
                        walkBlockedTime = 0;
                        isWalking = false;
                        Interfacing.PerformInterfaceStop(entity.graphicsHandle, entity.pos);
                    }
                    walkBlockedTime += dt;
                    return true;
                }
                isWalkBlocked = false;
                Level.S.SetPFBlockedMap(nextTile, WorldBlock.PFBlockType.DynamicBlocked);
                Level.S.SetPFBlockedMap(prevTile, WorldBlock.PFBlockType.Unblocked);
                pfBlockedTile = nextTile;

                Interfacing.PerformInterfaceMove(entity.graphicsHandle, nextTile, distToNextTile * invSpeed);
                isWalking = true;
            }

            float timeLeft = distToNextTile * invSpeed;
            if (timeLeft > dt)
            {
                distToNextTile -= dt * speed;
                onTileCenter = false;
            }
            else
            {
                prevTile = nextTile;
                distToNextTile = 1;
                onTileCenter = true;
                ++pathStart;

                if (pathEnd - pathStart > 0)
                {
                    OnUpdate(dt - timeLeft);
                }
                else
                {
                    Interfacing.PerformInterfaceStop(entity.graphicsHandle, prevTile);                    
                    isWalking = false;
                }
            }

            return true;
        }
	    
	    public void SetDest(HexXY dest, uint distToStop, uint blockedCost)
        {            
            this.dest = dest;
            this.distToStop = distToStop;
            this.blockedCost = blockedCost;            
            this.shouldRecalcPath = true;
        }

        public void SetSpeed(float speed)
        {
            this.speed = speed;
            this.invSpeed = 1.0f / speed;
        }
    }
}
