module runes;
import engine;
import std.stdio;
import utils;
import enums;
import logger;



class Rune : Entity, Fibered
{
	mixin Freelist;
	mixin _BoundFibers;
	mixin _ComponentsEventHandlers;

	override void update(float dt)
	{
		static string emitSwitchBody() 
		{
			string code;
			foreach(runeType; EnumMembers!RuneType)
			{
				auto st = to!string(runeType);
				code ~= q{case RuneType.} ~ st ~ q{: } ~ st[0].toLower ~ st[1..$] ~ q{Update(dt); break; } ~ "\n";
			}
			return code;			
		}

		//pragma(msg, emitSwitchBody);

		
		switch(cast(RuneType)entityType)
		{
		    mixin(emitSwitchBody);
		    default: assert(false);
		}

		Entity.update(dt);
	}

	void fRuneUpdate(float dt)
	{
		foreach(ent; worldBlock.entityMap[pos.x][pos.y].els())
		{
			Mob mob = cast(Mob)ent;
			if(mob !is null)		
			{				        
				float damage = dt * 5;
				mob.damage(damage);				
				mob.performInterfaceOp(EntityOperation.Damage, &damage);
				mob.updateInterfaceInfo();
			}
		}
	}

	void cRuneUpdate(float dt)
	{

	}
}


void place(RuneType runeType, HexXY p)
{
	auto rune = Rune.allocate(EntityClass.Rune, cast(int)runeType);
	rune.spawn(p);	
}

