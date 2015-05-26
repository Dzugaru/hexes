public enum TerrainCellType : byte
{
    Empty = 0,
    DryGround,
	Grass,
	FusedRuneStone,
    RuneStone
}

public enum EffectType
{
    YellowBlast,
    BlueBlast,
    FireBlast
}

public enum SpellType
{    
    LineOfFire,
    ColdCircle,
    FireTurret 
}

public enum EntityClass
{
    Character,
    Mech,
    Rune,
    Collectible,
    SpellEffect
}

public enum MechType
{
    StatueCaster,
    StatueTeachMelee,
    AvatarWalkPressPlate,
    Door
}

public enum CharacterType
{       
    Player,
    Spider, 
    
    AvatarLearn   
}

public enum InanimateType
{
    Stone,
    Tree
}

public enum SpellEffectType
{
    GroundFlame,
    Stone
}

public enum RuneType
{
    Nop = 0,

    Arrow0 = 1,
    ArrowCross = 2,
    ArrowL60 = 3,
    ArrowL120 = 4,
    ArrowR60 = 5,
    ArrowR120 = 6,    
    
    AvatarWalkDir = 101,
    AvatarWalkDirDraw = 102,
    AvatarForward = 103,
    AvatarForwardDraw = 104,
    AvatarForwardDupDraw = 105,
    AvatarLeft = 106,
    AvatarRight = 107,  
   
    If = 200,
    PredicateAvatarRef = 201,
    PredicateTileEmpty = 202,
    PredicateTileWall = 203,
    PredicateTileMonster = 204,

    Flame = 300,
    Stone = 301,
    Wind = 302,

    Number2 = 400,
    Number3 = 401,
    Number4 = 402,
    Number5 = 403,
    Number6 = 404,
    Number7 = 405,    
}

public enum CollectibleType
{
    Scroll,
    FireGem
}

public enum EntityOperation
{
    //Entities
    Spawn,
    Move,
    Stop,
    Attack,
    Damage,
    UpdateInfo,
    Die
}
