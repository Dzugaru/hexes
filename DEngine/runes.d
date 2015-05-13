module runes;
import engine;
import std.algorithm;

class Rune : Inanimate
{
	mixin Freelist;
	mixin _ComponentsEventHandlers;

	float power;
	uint dir;

	void construct(RuneType type, uint dir)
	{		
		Entity.construct(EntityClass.Rune, type);
		this.dir = dir;
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
		static struct Info { align: float power; uint dir; };
		auto info = Info(power, dir);
		performInterfaceOp(EntityOperation.UpdateInfo, &info);
	}
}

class CompiledRune
{
	RuneType type;
	uint dir;
	CompiledRune[6] neighs;
}

bool canDrawRune(Entity ent, RuneType rune, HexXY pos)
{
	return HexXY.dist(ent.pos, pos) == 1 &&
		//also can be done with typeid(el) == typeid(Rune)
		!any!(el => cast(Rune)el !is null)(worldBlock.entityMap[pos.x][pos.y].els());
	
}

void drawRune(Entity ent, RuneType type, HexXY pos)
{	
	auto runeData = data.runeDatas[type];
	uint dirIdx;
	if(runeData.isDirectional)			
		dirIdx = HexXY.dirs.countUntil(pos - ent.pos);		
	else
		dirIdx = 0;

	auto rune = Rune.allocate(type, dirIdx);
	rune.power = 1;
	rune.spawn(pos);	
}

bool canEraseRune(Entity ent, HexXY pos)
{
	return 
		HexXY.dist(ent.pos, pos) <= 1 &&
		any!(el => cast(Rune)el !is null)(worldBlock.entityMap[pos.x][pos.y].els());
}

void eraseRune(HexXY pos)
{
	auto entList = worldBlock.entityMap[pos.x][pos.y];
	auto rune = entList.els().find!(el => cast(Rune)el !is null).front;
	rune.die();
}
