module engine;
import std.conv;
import std.math;
import std.string;
public import math;
import noise;
import std.traits : EnumMembers;
import logger;
import utils;
import std.container;
import std.stdio;

@safe:

mixin(import("SharedEnums.cs"));

/***************************************************************************************************
* Sandbox
* 1 world block 10x10 for now
*/
WorldBlock!10 worldBlock;

void startTheWorld()
{
	//Setup logging

	worldBlock = new WorldBlock!10(HexXY(0,0));
	worldBlock.generate(BinaryNoiseFunc(Vector2(100, 200), 0.25f, 0.6f), 
						BinaryNoiseFunc(Vector2(200, 100), 0.25f, 0.4f));
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
final class WorldBlock(uint sz)
{
align:
	immutable HexXY position;

	TerrainCellType[sz][sz] cellTypes;
	int[terrainTypesCount] cellTypeCounts;
	int nonEmptyCellsCount;

	//Pathfinding support
	uint pfExpandMarker;
	uint[sz][sz] pfExpandMap;
	ubyte[sz][sz] pfStepsMap;

	this(HexXY position)
	{
		this.position = position;		
	}

	bool pfIsPassable(HexXY pos)
	{
		return pos.x >= 0 && pos.x < sz && pos.y >= 0 && pos.y < sz &&
			   cellTypes[pos.x][pos.y] != TerrainCellType.Empty;
	}

	float pfGetPassCost(HexXY pos)
	{
		return 1;
	}

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


		log(format("Block generated: %d non-empty cells", nonEmptyCellsCount));
		foreach(i, c; cellTypeCounts)
		{
		    log(format("%s %d", to!string(cast(TerrainCellType)i), c));
		}
	}
	void generateSolidFirstType()
	{
		for (int y = 0; y < sz; ++y)		
			for (int x = 0; x < sz; ++x)			
				cellTypes[x][y] = cast(TerrainCellType)1;		
	}
}

/***************************************************************************************************
* Pathfinding (simple A*)
*/
enum pfMaxFrontSize = 8192;

@trusted HexXY[] findPath(in HexXY from, in HexXY to, HexXY[] pathStorage)
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
			if(worldBlock.pfIsPassable(np) &&
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
	worldBlock = new WorldBlock!10(HexXY(0,0));
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
* Some "living" entity?
*/
class Character
{

}
