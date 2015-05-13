public enum TerrainCellType
{
    Empty = 0,
    DryGround,
	Grass,
	Snow	
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
    Inanimate,
    Rune,
    Collectible
}

public enum CharacterType
{    
    Pyramid,
    Sphere,
    Cube,
    Player,
    Spider,    
}

public enum InanimateType
{
    Stone,
    Tree
}

public enum RuneType
{
    Compile,

    Arrow0,
    ArrowCross,
    ArrowL60,
    ArrowL120,
    ArrowR60,
    ArrowR120,
    
    Avatar,
    AvatarWalkDir,
    AvatarWalkDirDraw,
    AvatarForward,
    AvatarForwardDraw,
    AvatarForwardDupDraw,
    AvatarLeft,
    AvatarRight, 
   
    If,
    PredicateAvatarRef,
    PredicateTileEmpty,
    PredicateTileWall,
    PredicateTileMonster,

    Flame,
    Stone,
    Wind,

    Number2,
    Number3,
    Number4,
    Number5,
    Number6,
    Number7,    
}

public enum CollectibleType
{
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
