public enum TerrainCellType
{
    Empty = 0,
	Grass,
	Snow	
}

public enum EffectType
{
    YellowBlast,
    BlueBlast
}

public enum SpellType
{    
    LineOfFire,
    ColdCircle        
}

public enum EntityClass
{
    Character,
    Rune
}

public enum CharacterType
{    
    Pyramid,
    Sphere,
    Cube,
    Player,
    Spider,    
}

public enum RuneType
{
    CRune,
    FRune,
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
