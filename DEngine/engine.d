module engine;
import std.conv;
import std.math;
import std.string;
public import math;
import noise;
import std.traits : EnumMembers;
import logger : log;
import utils;

@safe:

enum TerrainCellType
{
	Empty = 0,
	Grass,
	Snow
}

enum terrainTypesCount = EnumMembers!TerrainCellType.length;

struct HexXY
{
nothrow:
@nogc:
	static immutable Vector2 ex = Vector2(sqrt(3f) * 0.5f, 0.5f);
	static immutable Vector2 ey = Vector2(-sqrt(3f) * 0.5f, 0.5f);

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

final class WorldBlock(uint sz)
{
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


		log(wformat("Block generated: %d non-empty cells"w, nonEmptyCellsCount));
		foreach(i, c; cellTypeCounts)
		{
		    log(wformat("%s %d"w, to!string(cast(TerrainCellType)i), c));
		}
	}
}

class Character
{

}
