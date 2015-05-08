public enum TerrainCellType
{
    Empty = 0,
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
    CRune,
    FRune,   
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
