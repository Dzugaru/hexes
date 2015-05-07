/**
* Module, that controls everything in the game world,
* except player actions
*/

module overseer;
import std.random;
import engine;
import interfacing;
import std.stdio;

void start()
{
	rndGen.seed(1);
	foreach(i; 0..5)
	{

		HexXY p;
		do
		{
			p.x = std.random.uniform(0, worldBlocksSize);
			p.y = std.random.uniform(0, worldBlocksSize);
		} while(worldBlock.cellType(p) == TerrainCellType.Empty || HexXY.dist(player.pos, p) < 20 ||
				worldBlock.pfBlockedMap[p.x][p.y]);

		auto mob = Mob.allocate(data.mobDatas["spider"]);
		mob.spawn(p);		
	}


	//auto mob = Mob.allocate(data.mobDatas["spider"]);
	//mob.spawn(HexXY(2,2));

	auto gem = Collectible.allocate(CollectibleType.FireGem);
	gem.spawn(HexXY(2,2));	
	

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
			mixin(fibDelayWithoutBreak!q{0.5});
		}
	}

	//fibers.startFree(&disco);
}



void update(float dt)
{
}

unittest
{
	logger.logImpl = (msg) { writeln(msg); };
	frontendMock.setup();

	engine.startTheWorld();
	overseer.start();
	writeln("Overseer started");

	foreach(i; 0 .. 1000)
	{
		engine.update(0.1f);
		playerCast(HexXY(1,1));
	}
	
}
