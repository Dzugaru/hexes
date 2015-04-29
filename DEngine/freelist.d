/**
* Mixin for per-type freelist allocation/deallocation
* similar to http://wiki.dlang.org/Memory_Management#Free_Lists
*/

module freelist;
import std.traits;
import std.stdio;
import std.conv : to;

mixin template Freelist()
{
	private alias _FlT = typeof(this);

	debug private static uint _flCount; 
	private static _FlT _freelist;
	private _FlT _flNext;

	public static _FlT allocate(AA...)(AA args)
	{
		_FlT inst;
		if(_freelist)
		{
			inst = _freelist;
			_freelist = inst._flNext;
			debug --_flCount;
		}
		else
		{
			debug ++newCount;
			inst = new _FlT();
		}

		inst.reConstruct(args);
		return inst;
	}

	static if(hasMember!((BaseClassesTuple!_FlT)[0], "deallocate"))
	{
		public override void deallocate()
		{
			this._flNext = _freelist;
			_freelist = this;
			debug ++_flCount;
		}	
	}
	else
	{
		public void deallocate()
		{
			this._flNext = _freelist;
			_freelist = this;
			debug ++_flCount;
		}	
	}
}

private:
debug:

debug uint newCount = 0;
class A
{
	mixin Freelist;

	long[128] x;
	float a, b, c;	

	void reConstruct(float a, float b, float c)
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

	void reConstruct(string bl)
	{
		bla = to!string(blaCnt++) ~ bl;
		A.reConstruct(3.14, 2.7, 1.4);
	}
}

class C : B
{
	mixin Freelist;	

	override void reConstruct(string bl)
	{
		B.reConstruct(bl ~ "C");
	}
}

unittest
{
	auto c0 = C.allocate("bla0");
	auto c1 = C.allocate("bla1");
	auto c2 = C.allocate("bla2");
	c0.deallocate();
	c1.deallocate();
	auto c3 = C.allocate("bla3");
	A ac2 = c2;
	c2.deallocate(); //deallocating from base reference, should work
	auto c4 = C.allocate("bla4");

	auto a1 = A.allocate(1,2,3);
	a1.deallocate();
	auto a2 = A.allocate(3,4,5);

	assert(c3.bla == "3bla3C" && c3.x[42] == 84);
	assert(c4.bla == "4bla4C");
	assert(newCount == 4);
	assert(A._flCount == 0);
	assert(C._flCount == 1);
}

