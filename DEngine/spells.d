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
import data;
import std.algorithm : min, max;

//TODO: group (Entity ent, HexXY target)

SLList!(Spell, Spell.listNext) allSpells;

class Spell : Fibered
{
	mixin Freelist;
	Spell listNext;

	SpellType type;
	Entity ent;
	HexXY target;
	bool isLaunched, isInterrupted;
	float launchTime, launchTimeLeft;

private:
	mixin _BoundFibers;		

	void construct(Entity ent, SpellType type, HexXY target)
	{
		this.type = type;
		this.ent = ent;
		this.target = target;
		this.isLaunched = false;
	}

	void castIt()
	{
		launchTime = launchTimeLeft = data.spellDatas[type].launchTime;
		startLaunchFiber();
	}

	static string emitFiberSwitch(string fiberType)() 
	{
		string code;
		foreach(spellType; EnumMembers!SpellType)
		{
			immutable sst = to!string(spellType);			
			immutable string fiberFunc = sst[0].toLower ~ sst[1..$] ~ fiberType;			
			static if(__traits(compiles, mixin(fiberFunc ~ "()")))
				code ~= q{case SpellType.} ~ sst ~ q{: startFiber(&} ~ fiberFunc ~ q{); break; } ~ "\n";
		}
		return code;			
	}

	void startLaunchFiber()
	{
		pragma(msg, emitFiberSwitch!"LaunchFiber");

		switch(type)
		{
			mixin(emitFiberSwitch!"LaunchFiber");
			default: break;
		}
	}

	void startMainFiber()
	{
		pragma(msg, emitFiberSwitch!"MainFiber");	

		switch(type)
		{
			mixin(emitFiberSwitch!"MainFiber");
			default: assert(false);
		}		
	}	

	void update(float dt)
	{
		if(isInterrupted)
		{
			if(!isLaunched)
			{
				(cast(SpellCaster)ent).spellFinishedCasting();
			}

			//TODO: interruption fiber (all explodes!!!)? 
			fiberedDie();
			allSpells.remove(this);
			deallocate();
			return;
		}

		fiberedUpdate(dt);		

		if(!isLaunched)
		{
			launchTimeLeft = max(0, launchTimeLeft - dt);
			if(launchTimeLeft == 0)
			{
				isLaunched = true;
				(cast(SpellCaster)ent).spellFinishedCasting();
				startMainFiber();
			}
		}
		else
		{
			if(fibList.isEmpty())
			{
				allSpells.remove(this);
				deallocate();
			}
		}
	}

public:
	void interrupt()
	{
		isInterrupted = true;
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
	sp.castIt();
	return sp;
}


void update(float dt)
{
	foreach(sp; allSpells.els())
		sp.update(dt);
}

private:
bool canCastByDistance(Entity ent, HexXY target, uint minDist, uint maxDist)
{
	uint dist = HexXY.dist(ent.pos, target);
	return dist >= minDist && dist <= maxDist && worldBlock.pfIsPassable(target);
}

bool lineOfFireCanCast(Entity ent, HexXY target)
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

bool coldCircleCanCast(Entity ent, HexXY target)
{
	return canCastByDistance(ent, target, 1, 1);
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
