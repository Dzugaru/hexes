module data;
import enums;
import spells;
import engine;

static immutable MobData[string] mobDatas;
static immutable RuneData[RuneType] runeDatas;

immutable struct MobData
{
	CharacterType characterType;
	float maxHP;
	float speed;
	float attackDmgAppDelay; 
	float attackDur;
	float attackDamage;
}

immutable struct RuneData
{
	float placingTime;
}

static this()
{
	//											Graphics				HP		Speed	DmgDelay	AtkDuration		AtkDmg
	mobDatas["spider"]				= MobData(	CharacterType.Spider,		10,		1,		0.5,		1.0,			1		);
		
	//											Placing time
	runeDatas[RuneType.FRune]		= RuneData( 1.0f					);
	runeDatas[RuneType.CRune]		= RuneData( 1.0f					);
}
