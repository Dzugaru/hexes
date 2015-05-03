/**
* Module, that controls everything in the game world,
* except player actions
*/

module overseer;
import std.random;
import engine;
import interfacing;

void start()
{
	rndGen.seed(1);
	foreach(i; 0..25)
	{
		
		HexXY p;
		do
		{
			p.x = std.random.uniform(0, worldBlocksSize);
			p.y = std.random.uniform(0, worldBlocksSize);
		} while(worldBlock.cellType(p) == TerrainCellType.Empty && player.pos != p);
	
		auto mob = Mob.allocate(GrObjType.Spider, 1.0);
		mob.spawn(p);		
	}
	
	

	static void disco()
	{
		foreach(i; 0..100)
		{
			HexXY p;
			do
			{
				p.x = std.random.uniform(0, worldBlocksSize);
				p.y = std.random.uniform(0, worldBlocksSize);
			} while(worldBlock.cellType(p) == TerrainCellType.Empty);
			interfacing.cb.showEffectOnTile(p, EffectType.BlueBlast);
			fibers.delay(0.5);
		}
	}

	fibers.start(&disco);
}



void update(float dt)
{
}
