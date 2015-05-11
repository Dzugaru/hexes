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
						BinaryNoiseFunc(Vector2(200, 100), 0.25f, 0.0f));

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

	stopOrResumeTime();
}

bool isTimeStopped = false;

void stopOrResumeTime()
{
	bool shouldStop = !player.isWalking && spells.allSpells.isEmpty();

	if(isTimeStopped != shouldStop)
	{
		isTimeStopped = shouldStop;
		interfacing.cb.stopOrResumeTime(isTimeStopped);
	}	
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

	HexXY rotRight(HexXY center) const
	{
		int x = this.x - center.x;
		int y = this.y - center.y;
		HexXY nd;
		if(y == 0) nd = HexXY(0, -x);			 
		else if(x == 0) nd = HexXY(y, y);
		else if(x == y)	nd = HexXY(x, 0);		
		else assert(false);
		return center + nd;
	}

	HexXY rotLeft(HexXY center) const
	{
		int x = this.x - center.x;
		int y = this.y - center.y;
		HexXY nd;
		if(y == 0) nd = HexXY(x, x);			 
		else if(x == 0)	nd = HexXY(-y, 0);
		else if(x == y) nd = HexXY(0, y);		
		else assert(false);
		return center + nd;
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

	auto cellType(in HexXY p) const 
	{ 
		if(p.x >= 0 && p.x < sz && p.y >= 0 && p.y < sz)
			return cellTypes[p.x][p.y];
		else
			return TerrainCellType.Empty;
	}
	
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
						type = TerrainCellType.DryGround;
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

void damageEveryone(HexXY pos, float damage)
{
	foreach(ent; worldBlock.entityMap[pos.x][pos.y].els())				
	{
		HasHP hpEnt = cast(HasHP)ent;
		if(hpEnt !is null)	hpEnt.damage(damage);		
	}
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

	EntityHandle entityHandle;
	int entityType;

	abstract void componentsOnSpawn(HexXY pos);
	abstract void componentsOnUpdate(float dt);	
	abstract void componentsOnDie();
	abstract void componentsOnUpdateInterface();

	void construct(EntityClass cls, int type)
	{
		entityHandle = interfacing.cb.createEntity(cls, type);
		entityType = type;
	}

	void update(float dt)
	{
		//TODO: move this to CanWalk?
		HexXY prevPos = pos;

		componentsOnUpdate(dt);

		//TODO: move this to CanWalk? or it can be thrown by some other force...
		if(pos != prevPos)
		{
			bool removeSucceded = worldBlock.entityMap[prevPos.x][prevPos.y].remove(this);
			assert(removeSucceded);
			worldBlock.entityMap[pos.x][pos.y].insert(this);			
		}
	}

	void spawn(HexXY p)
	{
		pos = p;

		worldBlock.entityList.insert(this);
		worldBlock.entityMap[p.x][p.y].insert(this);	

		componentsOnSpawn(p);

		performInterfaceOp(EntityOperation.Spawn, &pos);
		updateInterface();		
	}

	void die()
	{
		//log("die");
		componentsOnDie();
		worldBlock.entityList.remove(this);
		worldBlock.entityMap[pos.x][pos.y].remove(this);		
		
		performInterfaceOp(EntityOperation.Die, null);
	}

	void updateInterface() 
	{
		componentsOnUpdateInterface();
	}

	void performInterfaceOp(EntityOperation op, void* args)
	{
		interfacing.cb.performOpOnEntity(entityHandle, op, args);
	}
}

mixin template _ComponentsEventHandlers()
{
	override void componentsOnSpawn(HexXY pos)
	{
		static if(isAssignable!(CanWalk, typeof(this)))	canWalkSpawn(pos);			
	}

	override void componentsOnUpdate(float dt)
	{
		static if(isAssignable!(HasHP, typeof(this))) if(hasHPUpdate(dt)) return; //stop updating all others
		static if(isAssignable!(CanWalk, typeof(this)))	canWalkUpdate(dt);
		static if(isAssignable!(Fibered, typeof(this)))	fiberedUpdate(dt);
	}

	override void componentsOnDie()
	{
		static if(isAssignable!(CanWalk, typeof(this))) canWalkDie();
		static if(isAssignable!(Fibered, typeof(this))) fiberedDie();			
	}

	override void componentsOnUpdateInterface()
	{
		static if(isAssignable!(HasHP, typeof(this)))
		{
			static struct Info
			{
			align:
				float currentHP, maxHP;
			}
			auto info = Info(this.currentHP, this.maxHP);
			performInterfaceOp(EntityOperation.UpdateInfo, &info);
		}		
	}
}

interface CanWalk 
{
}

mixin template _CanWalk(uint maxPathLen)
{
	static assert(isAssignable!(CanWalk, typeof(this)));

	//TODO: pack most of this to struct?
	HexXY[maxPathLen] pathStorage;
	HexXY[] path;
	HexXY prevTile, pfBlockedTile;
	Nullable!HexXY dest;
	uint blockedCost;
	bool onTileCenter;
	float speed, invSpeed, distToNextTile;
	bool isWalkBlocked, isWalking;
	float walkBlockedTime;
	bool shouldRecalcPath;
	bool shouldStopNearDest;

	void canWalkSpawn(HexXY pos)
	{
		prevTile = pos;
		distToNextTile = 1;
		onTileCenter = true;
		isWalkBlocked = isWalking = false;
		worldBlock.pfBlockedMap[pos.x][pos.y] = true;
		pfBlockedTile = pos;
	}	

	void canWalkDie()
	{
		worldBlock.pfBlockedMap[pfBlockedTile.x][pfBlockedTile.y] = false;
	}

	void canWalkUpdate(float dt)
	{		
		if(onTileCenter && shouldRecalcPath)
		{
			path = findPath(pos, dest, pathStorage, blockedCost);
			if(path is null)
			{
				//log("PATH NULL");
				return;
			}
			
			shouldRecalcPath = false;
			if(shouldStopNearDest && path.length == 1)
			{
				path.length = 0;
				if(isWalking)
				{
					performInterfaceOp(EntityOperation.Stop, &pos);
					isWalking = false;
				}
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
					isWalking = false;
					performInterfaceOp(EntityOperation.Stop, &pos); 
				}				
				walkBlockedTime += dt;				
				return; 
			}
			isWalkBlocked = false;
			worldBlock.pfBlockedMap[nextTile.x][nextTile.y] = true;
			worldBlock.pfBlockedMap[prevTile.x][prevTile.y] = false;
			pfBlockedTile = nextTile;

			//Animate movement
			struct TCbArgs { HexXY dest; float time; } 
			TCbArgs cbArgs = { nextTile, distToNextTile * invSpeed };			
			performInterfaceOp(EntityOperation.Move, &cbArgs);

			isWalking = true;
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
				canWalkUpdate(dt - timeLeft);			
			}
			else
			{
				performInterfaceOp(EntityOperation.Stop, &prevTile);	
				if(shouldStopNearDest) path.length = 0;
				isWalking = false;
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
		performInterfaceOp(EntityOperation.Damage, &dmg);
		updateInterface();
	}

	bool hasHPUpdate(float dt)
	{
		if(currentHP == 0)
		{
			die();
			return true;
		}
		else
		{
			return false;
		}
	}	
}

class Mob : Entity, CanWalk, Fibered, HasHP
{
	mixin Freelist;
	mixin _ComponentsEventHandlers;

	mixin _BoundFibers;	
	mixin _CanWalk!64;	
	mixin _HasHP;

	float attackDmgAppDelay, attackDur, attackDamage;
	bool isAttacking;

	void construct(MobData mobData)
	{		
		Entity.construct(EntityClass.Character, mobData.characterType);

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
			if(!isWalking && HexXY.dist(pos, player.pos) == 1)
			{
				isAttacking = true;
				performInterfaceOp(EntityOperation.Attack, &player.pos);
				startFiber(() 
				{
					with(fibCtx)
					{	
						auto oldPlayerPos = player.pos;
						mixin(fibDelay!q{attackDmgAppDelay});						
						if(player.pos == oldPlayerPos)
						{
							//Apply dmg						
							float dmg = attackDamage;
							player.performInterfaceOp(EntityOperation.Damage, &dmg);
							//TODO: refresh bar including new dot speed? (same with dotheal)
						}
						
						mixin(fibDelay!q{attackDur - attackDmgAppDelay});						
						isAttacking = false;
					}
				});
			}
		}

		Entity.update(dt);
	}
}

interface SpellCaster
{	
	void spellFinishedCasting();
}

class Player : Entity, CanWalk, SpellCaster
{
	mixin _ComponentsEventHandlers;
	mixin _CanWalk!64;

	Spell castingSpell;		
	void spellFinishedCasting()
	{
		castingSpell = null;
	}

	uint[EnumMembers!CollectibleType.length] gatheredCollectibles;

	this()
	{
		Entity.construct(EntityClass.Character, CharacterType.Player);

		setSpeed(2);		
	}

	override void update(float dt)
	{		
		//Spell casting
		if(castingSpell)
		{
			if(isWalking)
			{
				castingSpell.interrupt();
			}
			else
			{
				interfacing.guiData.cooldownBarValue = castingSpell.launchTimeLeft / castingSpell.launchTime;
			}
		}
		else
		{
			interfacing.guiData.cooldownBarValue = 0;
		}

		//Collectible gather
		if(isWalkBlocked)
		{
			foreach(ent; worldBlock.entityMap[path[0].x][path[0].y].els())
			{
				Collectible coll = cast(Collectible)ent;
				if(coll !is null)
				{
					coll.die();
					gatheredCollectibles[coll.entityType] += coll.amount;
				}
			}
		}

		//Update collectibles number in GUI
		guiData.fGemsCount = gatheredCollectibles[CollectibleType.FireGem];

		Entity.update(dt);
	}

	bool castSpell(SpellType type, HexXY p)
	{			
		bool isSuccess = !isWalking && !castingSpell && spells.canCastSpell(this, type, p);		
		if(isSuccess)
			castingSpell = spells.castSpell(this, type, p);
		return isSuccess;		
	}
}

class Inanimate : Entity
{
	mixin Freelist;
	mixin _ComponentsEventHandlers;

	void construct(InanimateType type)
	{		
		Entity.construct(EntityClass.Inanimate, type);
	}	

	override void spawn(HexXY p)
	{
		Entity.spawn(p);
		worldBlock.pfBlockedMap[p.x][p.y] = true;
	}

	override void die()
	{
		Entity.die();
		worldBlock.pfBlockedMap[pos.x][pos.y] = false;
	}
}

class Collectible : Inanimate
{
	mixin Freelist;
	mixin _ComponentsEventHandlers;

	uint amount;

	void construct(CollectibleType type, uint amount)
	{		
		Entity.construct(EntityClass.Collectible, type);
		this.amount = amount;
	}
}

class Rune : Inanimate
{
	mixin Freelist;
	mixin _ComponentsEventHandlers;

	float power;

	void construct(RuneType type)
	{		
		Entity.construct(EntityClass.Rune, type);
	}

	override void spawn(HexXY p)
	{
		Entity.spawn(p);		
	}

	override void die()
	{
		Entity.die();		
	}

	override void updateInterface()
	{	
		performInterfaceOp(EntityOperation.UpdateInfo, &power);
	}
}
