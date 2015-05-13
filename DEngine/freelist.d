/**
* Mixin for per-type freelist allocation/deallocation
* similar to http://wiki.dlang.org/Memory_Management#Free_Lists
*/

module freelist;

import std.conv : to;
import std.math;
import std.traits : hasMember, BaseClassesTuple;
public import std.stdio;

debug bool writelnLog = false;

mixin template Freelist(bool shouldReset = true)
{
	private alias _FlT = typeof(this);
	static assert(is(_FlT == class));

	debug static uint _flCount; 
	private static _FlT _freelist;
	private _FlT _flNext;
	private bool _flIsFree;

	public static _FlT allocate(AA...)(AA args)
	{
		_FlT inst;
		if(_freelist)
		{
			inst = _freelist;
			_freelist = inst._flNext;
			static if(shouldReset) resetObj(inst);
			debug if(writelnLog) writeln("Reuse " ~ typeof(this).stringof);
			debug --_flCount;
		}
		else
		{			
			debug ++newCount;
			debug if(writelnLog) writeln("New " ~ typeof(this).stringof);
			inst = new _FlT();
		}

		inst._flIsFree = false;
		inst.construct(args);
		return inst;
	}

	static if(hasMember!((BaseClassesTuple!_FlT)[0], "deallocate"))
	{
		public override void deallocate()
		{
			this._flNext = _freelist;
			_freelist = this;
			debug if(writelnLog) writeln("Delete " ~ typeof(this).stringof);
			debug ++_flCount;
		}	
	}
	else
	{
		public void deallocate()
		{
			debug assert(!this._flIsFree);
			this._flIsFree = true;
			this._flNext = _freelist;
			_freelist = this;
			debug if(writelnLog) writeln("Delete " ~ typeof(this).stringof);
			debug ++_flCount;
		}	
	}
}

/***************************
* "Zeroes" the memory owned by obj pointer
*/
void resetObj(T)(T obj) 
if (is(T == class)) 
{
	if (obj !is null) 
	{
		auto omem = *cast(void**)&obj;
		omem[0..__traits(classInstanceSize, T)] = typeid(T).init[];
	}
}

debug uint newCount = 0;

private:
debug:

class A
{
	mixin Freelist;

	long[128] x;
	float a, b, c;	

	void construct(float a, float b, float c)
	{
		foreach(i, ref e; x)		
			e = i * 2;				
		this.a = a;
		this.b = b;
		this.c = c;
	}
}

class B : A
{
	mixin Freelist;

	static int blaCnt = 0;
	string bla;

	void construct(string bl)
	{
		bla = to!string(blaCnt++) ~ bl;
		A.construct(3.14, 2.7, 1.4);
	}
}

class C : B
{
	mixin Freelist;	

	override void construct(string bl)
	{
		B.construct(bl ~ "C");
	}
}

class D : A
{
	mixin Freelist;

	void construct()
	{
	}
}


unittest
{
	newCount = 0;

	auto c0 = C.allocate("bla0");
	auto c1 = C.allocate("bla1");
	auto c2 = C.allocate("bla2");
	c0.deallocate();
	c1.deallocate();
	auto c3 = C.allocate("bla3");
	A ac2 = c2;
	ac2.deallocate(); //deallocating from base reference, should work
	auto c4 = C.allocate("bla4");

	auto a1 = A.allocate(1,2,3);
	a1.deallocate();
	auto a2 = A.allocate(3,4,5);

	assert(c3.bla == "3bla3C" && c3.x[42] == 84);
	assert(c4.bla == "4bla4C");
	assert(newCount == 4);
	assert(A._flCount == 0);
	assert(C._flCount == 1);

	D d1 = D.allocate();
	d1.a = 5.0;
	d1.x[0] = 42;
	d1.deallocate();
	D d2 = D.allocate();
	assert(isNaN(d2.a));
	assert(d2.x[0] == 0);
}

