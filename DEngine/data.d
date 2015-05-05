module data;
import enums;
import spells;
import engine;

static immutable MobData[string] mobDatas;
static immutable SpellData[string] spellDatas;

struct MobData
{
	GrObjType grType;
	float maxHP;
	float speed;
	float attackDmgAppDelay; 
	float attackDur;
	float attackDamage;
}

struct SpellData
{
	bool function(Entity, HexXY) canCast;
	void function() mainFiber;
}

static this()
{
	//											Graphics				HP		Speed	DmgDelay	AtkDuration		AtkDmg
	mobDatas["spider"]				= MobData(	GrObjType.Spider,		10,		1,		0.5,		1.0,			1		);


	//											Can cast					Fiber
	spellDatas["lineOfFire"] = SpellData(		&LineOfFire!().canCast,		&LineOfFire!().mainFiber);
}
