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
	

	auto mob2 = Mob.allocate(GrObjType.Spider, 1.0);
	mob2.spawn(HexXY(7,7));

	auto mob3 = Mob.allocate(GrObjType.Spider, 1.0);
	mob3.spawn(HexXY(7,2));	

	auto mob4 = Mob.allocate(GrObjType.Spider, 1.0);
	mob4.spawn(HexXY(8,3));

	
	mob2.setDest(HexXY(0,0));	
	mob3.setDest(HexXY(0,0));	
	mob4.setDest(HexXY(0,0));	

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
