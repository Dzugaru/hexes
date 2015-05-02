module engine;
import std.conv;
import std.math;
import std.string;
import std.traits;
import std.container;
import std.stdio;
import std.typecons;

import interfacing;
import frontendMock;
import overseer;

import logger;
import utils;
import noise;
public import math;
public import enums;
public import freelist;
public import fibers;

/***************************************************************************************************
* Sandbox
* 1 world block 10x10 for now
*/
immutable uint worldBlocksSize = 10;
WorldBlock worldBlock;

void startTheWorld()
{
	worldBlock = new WorldBlock(HexXY(0,0));
	worldBlock.generate(BinaryNoiseFunc(Vector2(100, 200), 0.25f, 0.6f), 
						BinaryNoiseFunc(Vector2(200, 100), 0.25f, 0.4f));

	overseer.start();
}

void update(float dt)
{	
	foreach(e; worldBlock.entityList.els())	
		e.update(dt);	

	overseer.update(dt);
	fibers.update(dt);
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

align:
	int x, y;

	pure static float distSqr(in HexXY a, in HexXY b)
	{
		return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y) - (a.x - b.x) * (a.y - b.y);
	}

	pure static float dist(in HexXY a, in HexXY b)
	{
		return sqrt(distSqr(a, b));
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

align:
	immutable HexXY position;

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

	//Terrain
	TerrainCellType[sz][sz] cellTypes;
	int[terrainTypesCount] cellTypeCounts;
	int nonEmptyCellsCount;
	
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
				!pfBlockedMap[pos.x][pos.y];
	}
	bool pfIsStaticPassable(HexXY pos)
	{
		return pos.x >= 0 && pos.x < sz && pos.y >= 0 && pos.y < sz &&
				cellTypes[pos.x][pos.y] != TerrainCellType.Empty;
	}
	float pfGetPassCost(HexXY pos)
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

alias findPathStatic = findPath!(WorldBlock.pfIsStaticPassable);
alias findPathDynamic = findPath!(WorldBlock.pfIsPassable);

HexXY[] findPath(alias passableFunc)(in HexXY from, in HexXY to, HexXY[] pathStorage)
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
		float cost, sumCostHeuristic;		
		int opCmp(XYCost rhs) { return sumCostHeuristic >= rhs.sumCostHeuristic ? -1 : 1; }	
	}
	
	float getHeuristic(HexXY pos)
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
			if(mixin(q{worldBlock.} ~ __traits(identifier, passableFunc) ~ q{(np)}) &&
			   worldBlock.pfExpandMap[np.x][np.y] < worldBlock.pfExpandMarker)
			{	
				worldBlock.pfExpandMap[np.x][np.y] = worldBlock.pfExpandMarker;	
				float cost = c.cost + worldBlock.pfGetPassCost(np);
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

	auto path = findPathStatic(HexXY(0,0), HexXY(0,2), pathStorage); //simple path around wall	
	assert(path == [HexXY(1,0), HexXY(2,1), HexXY(2,2), HexXY(1,2), HexXY(0,2)]);

	path = findPathStatic(HexXY(2,2), HexXY(2,2), pathStorage); //zero-length path	
	assert(path == []);

	worldBlock.cellTypes[1][0] = cast(TerrainCellType)0;
	path = findPathStatic(HexXY(0,0), HexXY(0,2), pathStorage); //no path anymore
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

		interfacing.cb.performOpOnGrObj(grHandle, GrObjOperation.Spawn, &pos);
	}
}

interface CanWalk
{	
}

mixin template _CompsEventHandlers()
{
	override void compsOnSpawn(HexXY pos)
	{
		static if(isAssignable!(CanWalk, typeof(this)))		
			canWalkOnSpawn(pos);		
	}

	override void compsOnUpdate(float dt)
	{
		static if(isAssignable!(CanWalk, typeof(this)))		
			move(dt);		
	}
}

mixin template _CanWalk(uint maxPathLen)
{
	static assert(isAssignable!(CanWalk, typeof(this)));

	HexXY[maxPathLen] pathStorage;
	HexXY[] path;
	HexXY prevTile;
	Nullable!HexXY dest;
	bool onTileCenter;
	float speed, invSpeed, distToNextTile;
	bool isWalkBlocked;

	void canWalkOnSpawn(HexXY pos)
	{
		prevTile = pos;
		distToNextTile = 1;
		onTileCenter = true;
		isWalkBlocked = false;
	}	

	void changePath()
	{
		path = findPathStatic(pos, dest, pathStorage);
		dest.nullify();
	}

	void move(float dt)
	{		
		if(onTileCenter && !dest.isNull)
			changePath();

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
					interfacing.cb.performOpOnGrObj(grHandle, GrObjOperation.Stop, null);
				}
				return; 
			}
			isWalkBlocked = false;
			worldBlock.pfBlockedMap[nextTile.x][nextTile.y] = true;

			//Animate movement
			struct TCbArgs { HexXY dest; float time; } 
			TCbArgs cbArgs = { nextTile, distToNextTile * invSpeed };			
			interfacing.cb.performOpOnGrObj(grHandle, GrObjOperation.Move, &cbArgs);
		}

		float timeLeft = distToNextTile * invSpeed;
		if(timeLeft > dt)
		{
			distToNextTile -= dt * speed;
			onTileCenter = false;
		}
		else
		{
			worldBlock.pfBlockedMap[prevTile.x][prevTile.y] = false;
			prevTile = nextTile;
			distToNextTile = 1;	
			onTileCenter = true;
			path = path[1..$];
			if(path.length > 0)
			{
				move(dt - timeLeft);			
			}
			else
			{
				interfacing.cb.performOpOnGrObj(grHandle, GrObjOperation.Stop, null);
			}
		}
	}

	//Path will be changed on next tile center
	void setDest(HexXY p)
	{
		dest = p;
	}

	void setSpeed(float speed)
	{
		this.speed = speed;
		this.invSpeed = 1. / speed;
	}
}

class Mob : Entity, CanWalk
{
	mixin Freelist;

	mixin _CompsEventHandlers;
	mixin _CanWalk!64;

	void construct(GrObjType grType, float speed)
	{		
		Entity.construct(grType);
		setSpeed(speed);
	}	
}

unittest
{
	frontendMock.setup();

	worldBlock = new WorldBlock(HexXY(0,0));
	worldBlock.generateSolidFirstType();
	worldBlock.cellTypes[0][1] = cast(TerrainCellType)0;
	worldBlock.cellTypes[1][1] = cast(TerrainCellType)0;
	
	auto mob1 = Mob.allocate(GrObjType.Sphere, 1.0);
	mob1.spawn(HexXY(0,0));
	mob1.setDest(HexXY(0,2));
	mob1.update(0.25);

	while(mob1.path.length > 0)
	{
		mob1.update(0.25);		
	}

	assert(mob1.pos == HexXY(0,2));
}
