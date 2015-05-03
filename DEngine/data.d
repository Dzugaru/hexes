module data;
import enums;

static immutable Mob[string] mobs;

struct Mob
{
	GrObjType grType;
	float maxHP;
	float speed;
	float attackDmgAppDelay; 
	float attackDur;
	float attackDamage;
}

static this()
{
	//						Graphics				HP		Speed	DmgDelay	AtkDuration		AtkDmg
	mobs["spider"] = Mob(	GrObjType.Spider,		10,		1,		0.5,		1.0,			1		);
}
