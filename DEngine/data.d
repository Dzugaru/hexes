module data;
import enums;
import spells;
import engine;

static immutable MobData[string] mobDatas;

//TODO: make a complex key (SpellType, Level)?
static immutable SpellData[SpellType] spellDatas;

immutable struct MobData
{
	CharacterType characterType;
	float maxHP;
	float speed;
	float attackDmgAppDelay; 
	float attackDur;
	float attackDamage;
}

immutable struct SpellData
{
	float launchTime;
}

static this()
{
	//											Graphics				HP		Speed	DmgDelay	AtkDuration		AtkDmg
	mobDatas["spider"]				= MobData(	CharacterType.Spider,	10,		1,	0.5,		1.0,			1		);
		
	//
	spellDatas[SpellType.LineOfFire] = SpellData(0.25f);
	spellDatas[SpellType.ColdCircle] = SpellData(1.0f);
	spellDatas[SpellType.FireTurret] = SpellData(2.0f);
}
