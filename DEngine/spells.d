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
		startMainFiber();
	}

	static string emitFiberSwitch() 
	{
		string code;
		foreach(spellType; EnumMembers!SpellType)
		{
			immutable sst = to!string(spellType);			
			immutable string fiberFunc = sst[0].toLower ~ sst[1..$] ~ "Fiber";			
			//static if(__traits(compiles, mixin(fiberFunc ~ "()")))
			code ~= q{case SpellType.} ~ sst ~ q{: startFiber(&} ~ fiberFunc ~ q{); break; } ~ "\n";
		}
		return code;			
	}

	void startMainFiber()
	{
		pragma(msg, emitFiberSwitch);	

		switch(type)
		{
			mixin(emitFiberSwitch);
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

		bool prevIsLaunched = isLaunched;

		fiberedUpdate(dt);		

		if(!isLaunched)
		{
			launchTimeLeft = max(0, launchTimeLeft - dt);
			if(launchTimeLeft == 0)
			{
				isLaunched = true;
				(cast(SpellCaster)ent).spellFinishedCasting();				
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

bool fireTurretCanCast(Entity ent, HexXY target)
{
	return canCastByDistance(ent, target, 1, 1);
}

void fireTurretFiber()
{
	with(Spell.fibCtx)
	{	
		float damage = 5;
		HexXY dir = target - ent.pos;		

		Rune rune = Rune.allocate(RuneType.FRune);	
		rune.power = 0;	
		rune.spawn(target);			
		scope(exit) rune.die();

		while(!isLaunched)
		{
			mixin(fibYield);
			rune.power = 1 - launchTimeLeft / launchTime;
			rune.updateInterface();
		}		

		HexXY[3] damagePos;
		damagePos[0] = target + dir;
		damagePos[1] = (target + dir).rotLeft(target);
		damagePos[2] = (target + dir).rotRight(target);

		do
		{			
			foreach(j; 0 .. 3)
			{
				if(!worldBlock.pfIsPassable(damagePos[j])) continue;
				interfacing.cb.showEffectOnTile(damagePos[j], EffectType.FireBlast);
				damageEveryone(damagePos[j], damage);
			}
			rune.power -= 0.1f;
			rune.updateInterface();
			mixin(fibDelay!"0.5");
		} while(rune.power > 0);			
	}
}

bool lineOfFireCanCast(Entity ent, HexXY target)
{
	return canCastByDistance(ent, target, 1, 1);
}	

void lineOfFireFiber()
{
	with(Spell.fibCtx)
	{
		while(!isLaunched) mixin(fibYield); //TODO: setting to show that launch phase is empty and there is no sense in context switch

		float damage = 5;
		HexXY dir = target - ent.pos;
		foreach(i; 0 .. 5)
		{
			interfacing.cb.showEffectOnTile(target, EffectType.FireBlast);							

			damageEveryone(target, damage);

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

void coldCircleFiber()
{	
	with(Spell.fibCtx)
	{
		while(!isLaunched) mixin(fibYield);

		float damage = 5;
		HexXY dir = target - ent.pos;
		foreach(i; 0 .. 5)
		{
			interfacing.cb.showEffectOnTile(target, EffectType.BlueBlast);							

			damageEveryone(target, damage);

			target += dir;

			if(!worldBlock.pfIsPassable(target))
				break;				

			mixin(fibDelay!q{0.1});
		}				
	}	
}
