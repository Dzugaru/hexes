using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.EntityComponents
{
    public class Walker : IEntityComponent
    {
        Entity entity;

        HexXY[] pathStorage;
        int pathLen;
        HexXY prevTile;
        HexXY? dest;
        uint blockedCost;
        bool onTileCenter;
        float speed, invSpeed, distToNextTile;
        bool isWalkBlocked;
        float walkBlockedTime;
        bool shouldRecalcPath;
        bool shouldStopNearDest;

        public Walker(Entity ent, int maxPathLen)
        {
            this.entity = ent;
            pathStorage = new HexXY[maxPathLen];
        }

        void OnSpawn(HexXY pos)
        {
            prevTile = pos;
            distToNextTile = 1;
            onTileCenter = true;
            isWalkBlocked = false;
            WorldBlock.S.pfBlockedMap[pos.x,pos.y] = true;
        }

        void OnUpdate(float dt)
        {
            if (onTileCenter && shouldRecalcPath)
            {
                pathLen = Pathfinder.FindPath(entity.pos, dest.Value, pathStorage, blockedCost);
                shouldRecalcPath = false;
                if (shouldStopNearDest && path.length == 1)
                {
                    path.length = 0;
                    performInterfaceOp(GrObjOperation.Stop, &pos);
                }
                isWalkBlocked = false;
            }

            if (path.length == 0) return;
            auto nextTile = path[0];

            if (distToNextTile > 0.5)
                pos = prevTile;
            else
                pos = path[0];

            if (onTileCenter)
            {
                if (worldBlock.pfBlockedMap[nextTile.x][nextTile.y])
                {
                    if (!isWalkBlocked)
                    {
                        isWalkBlocked = true;
                        walkBlockedTime = 0;
                        performInterfaceOp(GrObjOperation.Stop, &pos);
                    }
                    walkBlockedTime += dt;
                    return;
                }
                isWalkBlocked = false;
                worldBlock.pfBlockedMap[nextTile.x][nextTile.y] = true;
                worldBlock.pfBlockedMap[prevTile.x][prevTile.y] = false;

            //Animate movement
        struct TCbArgs { HexXY dest; float time; }
        TCbArgs cbArgs = { nextTile, distToNextTile * invSpeed };
            performInterfaceOp(GrObjOperation.Move, &cbArgs);
    }

    float timeLeft = distToNextTile * invSpeed;
		if(timeLeft > dt)
		{
			distToNextTile -= dt* speed;
    onTileCenter = false;
		}
		else
		{			
			prevTile = nextTile;
			distToNextTile = 1;	
			onTileCenter = true;
			path = path[1..$];			

			if(path.length > (shouldStopNearDest? 1 : 0))
			{
                walk(dt - timeLeft);			
			}
			else
			{
                performInterfaceOp(GrObjOperation.Stop, &prevTile);	
				if(shouldStopNearDest) path.length = 0;
			}			
		}
	}

	//Path will be changed on next tile center
	void setDest(HexXY dest, uint blockedCost, bool shouldStopNearDest)
{
    this.dest = dest;
    this.blockedCost = blockedCost;
    this.shouldStopNearDest = shouldStopNearDest;

    this.shouldRecalcPath = true;
}

void setSpeed(float speed)
{
    this.speed = speed;
    this.invSpeed = 1. / speed;
}
    }
}
