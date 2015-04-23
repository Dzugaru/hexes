module engine;
import std.conv;
import std.math;
import std.string;
public import math;
import noise;
import std.traits : EnumMembers;
import logger;
import utils;

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
nothrow:
@nogc:
	static immutable Vector2 ex = Vector2(sqrt(3f) * 0.5f, 0.5f);
	static immutable Vector2 ey = Vector2(-sqrt(3f) * 0.5f, 0.5f);

align:
	int x, y;

	this(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	pure static float distSqr(HexXY a, HexXY b)
	{
		return a.x * a.x + b.y * b.y - a.x * a.y;
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

	bool opEquals(HexXY rhs) const
	{
		return x == rhs.x && y == rhs.y;
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

	this(HexXY position)
	{
		this.position = position;		
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
				cellTypes[y][x] = type;
				++cellTypeCounts[type];
			}
		}


		log(format("Block generated: %d non-empty cells", nonEmptyCellsCount));
		foreach(i, c; cellTypeCounts)
		{
		    log(format("%s %d", to!string(cast(TerrainCellType)i), c));
		}
	}
}

/***************************************************************************************************
* Some "living" entity?
*/
class Character
{

}
