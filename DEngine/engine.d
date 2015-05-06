module engine;
import std.conv;
import std.math;
import std.string;
import std.traits;
import std.container;
import std.stdio;
import std.typecons;
import std.algorithm : min, max;

import interfacing;
import frontendMock;
import overseer;

import logger;
import utils;
import noise;
public import data;
public import math;
public import enums;
public import freelist;
public import fibers;
public import spells;

/***************************************************************************************************
* Sandbox
* 1 world block 10x10 for now
*/
immutable uint worldBlocksSize = 32;
WorldBlock worldBlock;
Player player;

void startTheWorld()
{
	worldBlock = new WorldBlock(HexXY(0,0));
	worldBlock.generate(BinaryNoiseFunc(Vector2(100, 200), 0.25f, 0.6f), 
						BinaryNoiseFunc(Vector2(200, 100), 0.25f, 0.4f));

	player = new Player();
	HexXY p = HexXY(0,0);
	//do
	//{
	//    p.x = std.random.uniform(0, worldBlocksSize);
	//    p.y = std.random.uniform(0, worldBlocksSize);
	//} while(worldBlock.cellType(p) == TerrainCellType.Empty);
	player.spawn(p);

	overseer.start();
}

void update(float dt)
{	
	foreach(e; worldBlock.entityList.els())	
		e.update(dt);	

	overseer.update(dt);
	fibers.updateFree(dt);
	spells.update(dt);
}

enum terrainTypesCount = EnumMembers!TerrainCellType.length;

/***************************************************************************************************
* Terrain tile
*/
struct HexXY
{
nothrow 
@nogc
{
	static immutable Vector2 ex = Vector2(sqrt(3f) * 0.5f, 0.5f);
	static immutable Vector2 ey = Vector2(-sqrt(3f) * 0.5f, 0.5f);
	static immutable HexXY[6] neighbours = [HexXY(1,0), HexXY(1,1), HexXY(0,1), HexXY(-1,0), HexXY(-1,-1), HexXY(0,-1)];

align {	int x, y; }

	pure static uint dist(in HexXY a, in HexXY b)
	{
		int x = a.x - b.x;
		int y = a.y - b.y;
		if((x < 0) == (y < 0))		
			return max(abs(x), abs(y));
		else
			return abs(x - y);		
	}

	Vector2 toPlaneCoordinates() const
	{
		return x * ex + y * ey;
	}

	HexXY opBinary(string op)(HexXY rhs) const
	if(op == "+" || op == "-")
	{
		return HexXY(mixin("x" ~ op ~ "rhs.x"), mixin("y" ~ op ~ "rhs.y"));
	}

	void opOpAssign(string op)(HexXY rhs)
	if(op == "+" || op == "-")
	{
		x = mixin("x" ~ op ~ "rhs.x");
		y = mixin("y" ~ op ~ "rhs.y");
	}

	bool opEquals(in HexXY rhs) const
	{
		return x == rhs.x && y == rhs.y;
	}
}
	//throw and gc
	string toString()
	{
		return format("(%d,%d)", x, y);
	}
}

/***************************************************************************************************
* Convenience struct for terrain generation
*/
struct BinaryNoiseFunc
{
nothrow:
@nogc:
	immutable Vector2 randomOffset;
	immutable float frequency;
	immutable float perlinThreshold;

	this(Vector2 randomOffset, float frequency, float threshold)
	{            
		this.randomOffset = randomOffset;
		this.frequency = frequency;
		this.perlinThreshold = 1 - threshold * 2;
	}

	bool opCall(Vector2 p) const
	{
		float noiseVal = perlin2D(p + randomOffset, frequency);
		return noiseVal > perlinThreshold;
	}
}

/***************************************************************************************************
* A block of terrain, consisting of (sz x sz) tiles
*/
final  class WorldBlock
{
	alias sz = worldBlocksSize;

align
{
	immutable HexXY position;	

	//Terrain
	TerrainCellType[sz][sz] cellTypes;	
	int[terrainTypesCount] cellTypeCounts;
	int nonEmptyCellsCount;
}

	auto cellType(in HexXY p) const { return cellTypes[p.x][p.y]; }
	
	//Entities
	SLList!(Entity, Entity.wbAllEntitiesNext) entityList;
	SLList!(Entity, Entity.wbEntityMapNext)[sz][sz] entityMap;

	//Pathfinding support
	uint pfExpandMarker;
	uint[sz][sz] pfExpandMap;
	ubyte[sz][sz] pfStepsMap;
	bool[sz][sz] pfBlockedMap; //TODO: flags enum for layers of blocking (ground, air, astral, etc.?)
	bool pfIsPassable(HexXY pos)
	{
		return pos.x >= 0 && pos.x < sz && pos.y >= 0 && pos.y < sz &&
			cellType(pos) != TerrainCellType.Empty;
	}
	
	uint pfGetPassCost(HexXY pos)
	{
		return 1;		
	}

	void pfCalcStaticBlocking()
	{	
		foreach(x; 0..sz)
			foreach(y; 0..sz)
				pfBlockedMap[x][y] = cellTypes[x][y] == TerrainCellType.Empty;
	}

	this(HexXY position)
	{
		this.position = position;		
	}	

	//TODO:
	static void dispatchToBlock(alias func, AA...)(HexXY globalPos, AA args)
	{
		HexXY localPos;
		auto blockX = globalPos.x < 0 ? (-globalPos.x - 1) / sz - 1 : globalPos.x / sz;
		auto blockY = globalPos.y < 0 ? (-globalPos.y - 1) / sz - 1 : globalPos.y / sz;
		localPos.x = globalPos.x - blockX * sz;
		localPos.y = globalPos.y - blockY * sz;
		//getBlock(blockX, blockY).func(localPos, args);
	}

	//Generation
	void generate(in BinaryNoiseFunc nonEmpty, in BinaryNoiseFunc snow)
	{
		nonEmptyCellsCount = 0;
		foreach(ref e; cellTypeCounts)		
			e = 0;		

		for (int y = 0; y < sz; ++y)
		{
			for (int x = 0; x < sz; ++x)
			{
				HexXY c = position + HexXY(x, y);
				Vector2 p = c.toPlaneCoordinates(); 				
				TerrainCellType type;
				if (nonEmpty(p))
				{                        
					if (snow(p))
					{
						type = TerrainCellType.Snow;
					}
					else
					{
						type = TerrainCellType.Grass;
					}
					++nonEmptyCellsCount;
				}
				else
				{
					type = TerrainCellType.Empty;
				}
				cellTypes[x][y] = type;
				++cellTypeCounts[type];
			}
		}

		pfCalcStaticBlocking();

		log(format("Block generated: %d non-empty cells", nonEmptyCellsCount));
		foreach(i, c; cellTypeCounts)
		{
		    log(format("%s %d", to!string(cast(TerrainCellType)i), c));
		}
	}
	void generateSolidFirstType()
	{
		foreach(ref e; cellTypeCounts)		
			e = 0;	
		nonEmptyCellsCount = cellTypeCounts[1] = sz * sz;		

		for (int y = 0; y < sz; ++y)		
			for (int x = 0; x < sz; ++x)			
				cellTypes[x][y] = cast(TerrainCellType)1;		
	}
}

/***************************************************************************************************
* Pathfinding (simple A*)
*/
enum pfMaxFrontSize = 8192;

HexXY[] findPath(in HexXY from, in HexXY to, HexXY[] pathStorage, in uint blockedCost = 0)
{
	static immutable struct Step 
	{ 
		int dx, dy; 
		ubyte backIdx;
		HexXY opBinaryRight(string op : "+")(HexXY lhs)
		{
			return HexXY(lhs.x + dx, lhs.y + dy);
		}
	}

	static immutable Step[6] steps = [Step(1,0,3), Step(1,1,4), Step(0,1,5), Step(-1,0,0), Step(-1,-1,1), Step(0,-1,2)];

	static struct XYCost
	{ 
		HexXY p; 
		uint len;
		uint cost, sumCostHeuristic;
		int opCmp(XYCost rhs) { return sumCostHeuristic >= rhs.sumCostHeuristic ? -1 : 1; }	
	}
	
	uint getHeuristic(HexXY pos)
	{
		return HexXY.dist(pos, to);
	}

	static XYCost[pfMaxFrontSize] frontStore;

	auto front = BinaryHeap!(XYCost[])(frontStore, 0);
	front.insert(XYCost(from, 0, 0, getHeuristic(from)));

	//TODO: assume we're in the single worldblock for now
	++worldBlock.pfExpandMarker;	
	worldBlock.pfExpandMap[from.x][from.y] = worldBlock.pfExpandMarker;

	XYCost c;
	bool isFound = false;

	do
	{
		c = front.front;
		front.removeFront(); //BUG: removeAny documentation is broken!

		if(c.p == to)
		{
			isFound = true;
			break;
		}
		
		foreach(st; steps)
		{
			auto np = c.p + st;
			if(worldBlock.pfIsPassable(np) &&
			   worldBlock.pfExpandMap[np.x][np.y] < worldBlock.pfExpandMarker)
			{	
				worldBlock.pfExpandMap[np.x][np.y] = worldBlock.pfExpandMarker;	
				uint cost = c.cost + worldBlock.pfGetPassCost(np);

				if(blockedCost > 0 && np != to && worldBlock.pfBlockedMap[np.x][np.y]) 
					cost += blockedCost;				   

				auto n = XYCost(np, c.len + 1, cost, cost + getHeuristic(np));
				front.insert(n);
				worldBlock.pfStepsMap[np.x][np.y] = cast(ubyte)st.backIdx;
			}
		}
	} while(front.length > 0);

	if(isFound && c.len <= pathStorage.length)
	{
		uint pathLen = c.len;
		HexXY p = c.p;
		foreach(i; 0 .. pathLen)
		{
			pathStorage[pathLen - i - 1] = p;
			auto backIdx = worldBlock.pfStepsMap[p.x][p.y];
			p = p + steps[backIdx];
		}
		return pathStorage[0 .. pathLen];
	}
	else
	{
		return null;
	}
}

unittest
{
	frontendMock.setup();

	worldBlock = new WorldBlock(HexXY(0,0));
	worldBlock.generateSolidFirstType();
	worldBlock.cellTypes[0][1] = cast(TerrainCellType)0;
	worldBlock.cellTypes[1][1] = cast(TerrainCellType)0;
	auto pathStorage = new HexXY[128];

	auto path = findPath(HexXY(0,0), HexXY(0,2), pathStorage); //simple path around wall	
	assert(path == [HexXY(1,0), HexXY(2,1), HexXY(2,2), HexXY(1,2), HexXY(0,2)]);

	path = findPath(HexXY(2,2), HexXY(2,2), pathStorage); //zero-length path	
	assert(path == []);

	worldBlock.cellTypes[1][0] = cast(TerrainCellType)0;
	path = findPath(HexXY(0,0), HexXY(0,2), pathStorage); //no path anymore
	assert(path is null);
}


/***************************************************************************************************
* Something, that is physically present on a hex grid
*/
abstract class Entity
{	
	//Lists support
	Entity wbEntityMapNext, wbAllEntitiesNext;	

public:
	//Entity can span multiple tiles, but has a single coordinate
	HexXY pos;

	GrObjHandle grHandle;

	abstract void compsOnSpawn(HexXY pos);
	abstract void compsOnUpdate(float dt);	
	abstract void compsOnDie();

	void construct(GrObjType type)
	{
		grHandle = interfacing.cb.createGrObj(GrObjClass.Entity, type);
	}

	void update(float dt)
	{
		HexXY prevPos = pos;

		compsOnUpdate(dt);

		if(pos != prevPos)
		{
			worldBlock.entityMap[prevPos.x][prevPos.y].remove(this);
			worldBlock.entityMap[pos.x][pos.y].insert(this);			
		}
	}

	void spawn(HexXY p)
	{
		pos = p;

		worldBlock.entityList.insert(this);
		worldBlock.entityMap[p.x][p.y].insert(this);	

		compsOnSpawn(p);

		performInterfaceOp(GrObjOperation.Spawn, &pos);
		updateInterfaceInfo();		
	}

	void die()
	{
		log("die");
		compsOnDie();
		worldBlock.entityList.remove(this);
		worldBlock.entityMap[pos.x][pos.y].remove(this);

		performInterfaceOp(GrObjOperation.Die, null);
	}

	void performInterfaceOp(GrObjOperation op, void* args)
	{
		interfacing.cb.performOpOnGrObj(grHandle, op, args);
	}

	void updateInterfaceInfo() {}
}

mixin template _CompsEventHandlers()
{
	override void compsOnSpawn(HexXY pos)
	{
		static if(isAssignable!(CanWalk, typeof(this)))		
			walkSpawnInit(pos);		
	}

	override void compsOnUpdate(float dt)
	{
		static if(isAssignable!(HasHP, typeof(this)))		
		{
			if(currentHP == 0) die();
		}

		static if(isAssignable!(CanWalk, typeof(this)))		
			walk(dt);

		static if(isAssignable!(Fibered, typeof(this)))
			updateFibers(dt);
	}

	override void compsOnDie()
	{
		static if(isAssignable!(Fibered, typeof(this)))
			deallocateRunningFibers();
	}

	override void updateInterfaceInfo()
	{
		static if(isAssignable!(HasHP, typeof(this)))
		{
			static struct Info
			{
			align:
				float currentHP, maxHP;
			}
			auto info = Info(this.currentHP, this.maxHP);
			performInterfaceOp(GrObjOperation.UpdateInfo, &info);
		}		
	}
}

interface CanWalk {}
mixin template _CanWalk(uint maxPathLen)
{
	static assert(isAssignable!(CanWalk, typeof(this)));

	//TODO: pack most of this to struct?
	HexXY[maxPathLen] pathStorage;
	HexXY[] path;
	HexXY prevTile;
	Nullable!HexXY dest;
	uint blockedCost;
	bool onTileCenter;
	float speed, invSpeed, distToNextTile;
	bool isWalkBlocked;
	float walkBlockedTime;
	bool shouldRecalcPath;
	bool shouldStopNearDest;

	void walkSpawnInit(HexXY pos)
	{
		prevTile = pos;
		distToNextTile = 1;
		onTileCenter = true;
		isWalkBlocked = false;
		worldBlock.pfBlockedMap[pos.x][pos.y] = true;
	}	

	void walk(float dt)
	{		
		if(onTileCenter && shouldRecalcPath)
		{
			path = findPath(pos, dest, pathStorage, blockedCost);		
			shouldRecalcPath = false;
			if(shouldStopNearDest && path.length == 1)
			{
				path.length = 0;
				performInterfaceOp(GrObjOperation.Stop, &pos);
			}
			isWalkBlocked = false;
		}

		if(path.length == 0) return;
		auto nextTile = path[0];

		if(distToNextTile > 0.5)
			pos = prevTile;
		else
			pos = path[0];

		if(onTileCenter)
		{
			if(worldBlock.pfBlockedMap[nextTile.x][nextTile.y])
			{
				if(!isWalkBlocked)
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
			distToNextTile -= dt * speed;
			onTileCenter = false;
		}
		else
		{			
			prevTile = nextTile;
			distToNextTile = 1;	
			onTileCenter = true;
			path = path[1..$];			

			if(path.length > (shouldStopNearDest ? 1 : 0))
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

interface HasHP 
{
	void damage(float dmg);
}

mixin template _HasHP()
{
	static assert(isAssignable!(HasHP, typeof(this)));

	float currentHP, maxHP;

	void damage(float dmg)
	{
		currentHP = max(0, currentHP - dmg);
	}
}



class Mob : Entity, CanWalk, Fibered, HasHP
{
	mixin Freelist;
	mixin _BoundFibers;

	mixin _CompsEventHandlers;
	mixin _CanWalk!64;	
	mixin _HasHP;

	float attackDmgAppDelay, attackDur, attackDamage;
	bool isAttacking;

	void construct(MobData mobData)
	{		
		Entity.construct(mobData.grType);
		setSpeed(mobData.speed);
		attackDmgAppDelay = mobData.attackDmgAppDelay;
		attackDur = mobData.attackDur;
		attackDamage = mobData.attackDamage;
		maxHP = currentHP = mobData.maxHP;
		isAttacking = false;		
	}	

	override void update(float dt)
	{
		if(!isAttacking)
		{
			//Simple walk to player
			if(dest.isNull() || dest != player.pos)
			{
				setDest(player.pos, 0, true);
			}

			//Is we're blocked for some time try to find a way around other mobs	
			if(isWalkBlocked && walkBlockedTime > 0.5f) //TODO: random time?
			{
				//log(format("%s %f", isWalkBlocked, walkBlockedTime));
				setDest(player.pos, 10, true);
			}

			//Attack
			if(onTileCenter && HexXY.dist(pos, player.pos) == 1)
			{
				isAttacking = true;
				performInterfaceOp(GrObjOperation.Attack, &player.pos);
				startFiber(() 
				{					
					with(fibCtx)
					{						
						mixin(fibDelay!q{attackDmgAppDelay});
						//Apply dmg						
						float dmg = attackDamage;
						player.performInterfaceOp(GrObjOperation.Damage, &dmg);
						//TODO: refresh bar including new dot speed? (same with dotheal)
						mixin(fibDelay!q{attackDur - attackDmgAppDelay});						
						isAttacking = false;
					}
				});
			}
		}

		Entity.update(dt);
	}
}

class Player : Entity, CanWalk
{
	mixin _CompsEventHandlers;
	mixin _CanWalk!64;

	float spellGcd;
	Nullable!SpellData nextSpellData;
	HexXY nextSpellPos;

	this()
	{
		Entity.construct(GrObjType.Player);
		setSpeed(2);
		spellGcd = 0;
	}

	override void update(float dt)
	{		
		if(onTileCenter)
		{
			if(!nextSpellData.isNull() && spellGcd == 0)	
			{
				if(nextSpellData.canCast(this, nextSpellPos))				
				{					
					spells.castSpell(this, nextSpellPos, nextSpellData.mainFiber);	
					spellGcd = 0.5f;
				}

				nextSpellData.nullify();
			}
		}

		spellGcd = max(0, spellGcd - dt);

		Entity.update(dt);
	}

	void castSpell(HexXY p)
	{			
		if(nextSpellData.isNull())
		{
			nextSpellData = spellDatas["lineOfFire"];
			nextSpellPos = p;			
		}		
	}
}
