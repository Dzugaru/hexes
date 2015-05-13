module data;
import enums;
import spells;
import engine;

static immutable MobData[string] mobDatas;

//TODO: make a complex key (SpellType, Level)?
static immutable SpellData[SpellType] spellDatas;

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

immutable struct SpellData
{
	float launchTime;
}

immutable struct RuneData
{
	bool isDirectional;
}

static this()
{
	//											Graphics				HP		Speed	DmgDelay	AtkDuration		AtkDmg
	mobDatas["spider"]				= MobData(	CharacterType.Spider,	10,		1,	0.5,		1.0,			1		);
		
	//
	spellDatas[SpellType.LineOfFire] = SpellData(0.25f);
	spellDatas[SpellType.ColdCircle] = SpellData(1.0f);
	spellDatas[SpellType.FireTurret] = SpellData(2.0f);

	//													Directional?
	runeDatas[RuneType.Compile] =				RuneData(false);

	runeDatas[RuneType.Arrow0] =				RuneData(true);
	runeDatas[RuneType.ArrowL60] =				RuneData(true);
	runeDatas[RuneType.ArrowL120] =				RuneData(true);
	runeDatas[RuneType.ArrowR60] =				RuneData(true);
	runeDatas[RuneType.ArrowR120] =				RuneData(true);
	runeDatas[RuneType.ArrowCross] =			RuneData(true);

	runeDatas[RuneType.Avatar] =				RuneData(false);
	runeDatas[RuneType.AvatarWalkDir] =			RuneData(true);
	runeDatas[RuneType.AvatarWalkDirDraw] =		RuneData(true);	
	runeDatas[RuneType.AvatarForward] =			RuneData(false);
	runeDatas[RuneType.AvatarForwardDraw] =		RuneData(false);
	runeDatas[RuneType.AvatarForwardDupDraw] =	RuneData(false);
	runeDatas[RuneType.AvatarLeft] =			RuneData(false);
	runeDatas[RuneType.AvatarRight] =			RuneData(false);

	runeDatas[RuneType.If] =					RuneData(true);
	runeDatas[RuneType.PredicateAvatarRef] =	RuneData(false);
	runeDatas[RuneType.PredicateTileEmpty] =	RuneData(false);
	runeDatas[RuneType.PredicateTileWall] =		RuneData(false);
	runeDatas[RuneType.PredicateTileMonster] =	RuneData(false);

	runeDatas[RuneType.Flame] =					RuneData(false);
	runeDatas[RuneType.Stone] =					RuneData(false);
	runeDatas[RuneType.Wind] =					RuneData(false);

	runeDatas[RuneType.Number2] =				RuneData(false);
	runeDatas[RuneType.Number3] =				RuneData(false);
	runeDatas[RuneType.Number4] =				RuneData(false);
	runeDatas[RuneType.Number5] =				RuneData(false);
	runeDatas[RuneType.Number6] =				RuneData(false);
	runeDatas[RuneType.Number7] =				RuneData(false);
}
