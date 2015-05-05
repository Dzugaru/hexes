module spells;
import engine;
import std.stdio;
import std.random;
import gctrack;
import core.memory;

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


template TreeDeath()
{
	bool canCast(Entity ent, HexXY target)
	{
		return HexXY.dist(ent.pos, target) == 1;
	}

	class Branch
	{
		HexXY p;
		mixin Freelist!false;
		void construct(HexXY p)
		{
			this.p = p;
		}

		void fiber()
		{
			with(Spell.fibCtx)
			{
				//mixin(fibBreak);

				//writeln("Boom at " ~ to!string(p));
				//interfacing.cb.showEffectOnTile(target, EffectType.BlueBlast);

				mixin(fibDelay!q{0.5});
				//writeln("Boom at after" ~ to!string(p));

				//foreach(d; HexXY.neighbours)
				//{
					HexXY n = p + HexXY.neighbours[std.random.uniform(0,6)]; 
					//if(worldBlock.cellTypes[n.x][n.y] == TerrainCellTypes.Empty) continue;
					//if(std.random.uniform01() < 1 / 5.)
					//{						
						Spell.fibCtx.startFiber(&Branch.allocate(n).fiber);
					//}
				//}
				Branch.deallocate();
			}
		}
	}

	void mainFiber()
	{
		with(Spell.fibCtx)
		{
			//mixin(fibBreak);

			startFiber(&Branch.allocate(target).fiber);
		}
	}
}

unittest
{	
	//freelist.writelnLog = true;
	GC.disable();

	auto sp = castSpell(null, HexXY(0,0), &TreeDeath!().mainFiber);
	
	foreach(i; 0..10000000)
	{
	    //writeln("Time " ~ to!string(i * 0.5f));
	    //update(0.5f);
		
		//readln();
		sp.updateFibers(0.5f);
		//writeln(sp.fibList.count);
	}
	GC.enable();
	//freelist.writelnLog = false;
}