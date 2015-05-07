module spells;
import engine;
import std.stdio;
import std.random;
//import gctrack;
import core.memory;
import logger;
import std.string;
import std.ascii : toLower;
import std.traits : EnumMembers;

//TODO: group (Entity ent, HexXY target)

SLList!(Spell, Spell.listNext) allSpells;

class Spell : Fibered
{
	mixin Freelist;
	mixin _BoundFibers;

	Spell listNext;

	SpellType type;
	Entity ent;
	HexXY target;

	void construct(Entity ent, SpellType type, HexXY target)
	{
		this.type = type;
		this.ent = ent;
		this.target = target;
	}

	void startMainFiber()
	{
		static string emitSwitchBody() 
		{
			string code;
			foreach(spellType; EnumMembers!SpellType)
			{
				auto sst = to!string(spellType);
				code ~= q{case SpellType.} ~ sst ~ q{: startFiber(&} ~ sst[0].toLower ~ sst[1..$] ~ q{MainFiber); break; } ~ "\n";
			}
			return code;			
		}

		//pragma(msg, emitSwitchBody);
		
		switch(type)
		{
			mixin(emitSwitchBody);
			default: assert(false);
		}		
	}	
}



void update(float dt)
{
	foreach(sp; allSpells.els())
	{
		sp.fiberedUpdate(dt);
		if(sp.fibList.isEmpty())
		{
			sp.deallocate();
			allSpells.remove(sp);
		}
	}
}

bool canCastSpell(Entity ent, SpellType type, HexXY target)
{
	static string emitSwitchBody() 
	{
		string code;
		foreach(spellType; EnumMembers!SpellType)
		{
			auto sst = to!string(spellType);
			code ~= q{case SpellType.} ~ sst ~ q{: return(} ~ sst[0].toLower ~ sst[1..$] ~ q{CanCast(ent, target));} ~ "\n";
		}
		return code;			
	}

	//pragma(msg, emitSwitchBody);

	switch(type)
	{
		mixin(emitSwitchBody);
		default: assert(false);
	}	
}

Spell castSpell(Entity ent, SpellType type, HexXY target)
{
	Spell sp = Spell.allocate(ent, type, target);
	allSpells.insert(sp);
	sp.startMainFiber();
	return sp;
}

bool canCastByDistance(Entity ent, HexXY target, uint minDist, uint maxDist)
{
	uint dist = HexXY.dist(ent.pos, target);
	return dist >= minDist && dist <= maxDist && worldBlock.pfIsPassable(target);
}

bool lineOfFireCanCast(Entity ent, HexXY target)
{
	return canCastByDistance(ent, target, 1, 1);
}	

bool coldCircleCanCast(Entity ent, HexXY target)
{
	return canCastByDistance(ent, target, 1, 1);
}	

void lineOfFireMainFiber()
{
	with(Spell.fibCtx)
	{
		float damage = 5;
		HexXY dir = target - ent.pos;
		foreach(i; 0 .. 5)
		{
			interfacing.cb.showEffectOnTile(target, EffectType.BlueBlast);							

			foreach(ent; worldBlock.entityMap[target.x][target.y].els())				
			{
				HasHP hpEnt = cast(HasHP)ent;
				if(hpEnt !is null)		
				{				        
					hpEnt.damage(damage);				
					ent.performInterfaceOp(EntityOperation.Damage, &damage);
					ent.updateInterfaceInfo();
				}
			}

			target += dir;

			if(!worldBlock.pfIsPassable(target))
				break;				

			mixin(fibDelay!q{0.1});
		}				
	}
}

void coldCircleMainFiber()
{	
	with(Spell.fibCtx)
	{
		float damage = 5;
		HexXY dir = target - ent.pos;
		foreach(i; 0 .. 5)
		{
			interfacing.cb.showEffectOnTile(target, EffectType.BlueBlast);							

			foreach(ent; worldBlock.entityMap[target.x][target.y].els())				
			{
				HasHP hpEnt = cast(HasHP)ent;
				if(hpEnt !is null)		
				{				        
				    hpEnt.damage(damage);				
				    ent.performInterfaceOp(EntityOperation.Damage, &damage);
					ent.updateInterfaceInfo();
				}
			}

			target += dir;

			if(!worldBlock.pfIsPassable(target))
				break;				

			mixin(fibDelay!q{0.1});
		}				
	}	
}
