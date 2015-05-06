module spells;
import engine;
import std.stdio;
import std.random;
//import gctrack;
import core.memory;
import logger;
import std.string;

//TODO: use type instead of (Entity ent, HexXY target), Spell will become Spell(TArgs)

class Spell : Fibered
{
	mixin Freelist;
	mixin _BoundFibers;

	Spell listNext;

	Entity ent;
	HexXY target;

	void construct(Entity ent, HexXY target)
	{
		this.ent = ent;
		this.target = target;
	}
}

SLList!(Spell, Spell.listNext) allSpells;

void update(float dt)
{
	foreach(sp; allSpells.els())
	{
		sp.updateFibers(dt);
		if(sp.fibList.isEmpty())
		{
			sp.deallocate();
			allSpells.remove(sp);
		}
	}
}

Spell castSpell(Entity ent, HexXY target, void function() mainFiber)
{
	Spell sp = Spell.allocate(ent, target);
	allSpells.insert(sp);
	sp.startFiber(mainFiber);
	return sp;
}


template LineOfFire()
{
	uint counter = 0;

	bool canCast(Entity ent, HexXY target)
	{
		return HexXY.dist(ent.pos, target) == 1 && worldBlock.pfIsPassable(target);
	}	

	void mainFiber()
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
				        ent.performInterfaceOp(GrObjOperation.Damage, &damage);
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
}