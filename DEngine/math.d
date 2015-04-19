module math;
import std.conv;
import std.stdio;
import std.string;
import std.math : sqrt;
@safe:

alias Vector2 = Vector!(2, float);
alias Vector3 = Vector!(3, float);

struct Vector(uint sz, T : double)
if(sz >= 1 && !is(T == char))
{
nothrow @nogc
{
	private T[sz] c;
	//alias c this;

	mixin(emitXYZMembers());

	this(T[] c ...)
	{
		//BUG: https://issues.dlang.org/show_bug.cgi?id=14463
		//this.c = c will not work right with immutable Vector
		this.c[] = c[];
	}

	auto opBinary(string op)(in Vector!(sz,T) rhs)
	if(op == "+" || op == "-")
	{		
		Vector!(sz,T) res;
		foreach(i, ref e; res.c)		
			e = mixin("c[i]" ~ op ~ "rhs.c[i]");		
		return res;
	}

	auto opBinary(string op : "*")(in T rhs)
	{
		Vector!(sz,T) res;
		foreach(i, ref e; res.c)		
			e *= rhs;		
		return res;
	}

	auto opBinaryRight(string op : "*")(in T rhs)
	{
		Vector!(sz,T) res;
		foreach(i, ref e; res.c)		
			e *= rhs;		
		return res;
	}

	auto opOpAssign(string op : "*")(in T rhs)
	{
		foreach(i, ref e; c)		
			e *= rhs;		
	}

	@property auto normalized()
	{
		T len = 0;
		foreach(i; 0 .. sz)
			len += c[i] * c[i];		

		len = sqrt(len);

		Vector!(sz, T) res;
		foreach(i; 0 .. sz)
			res.c[i] = c[i] / len;

		return res;
	}

	auto dot(Vector!(sz, T) rhs)
	{
		T sum = 0;
		foreach(i; 0 .. sz)
			sum += c[i] * rhs.c[i];
		return sum;
	}
}

	//throw and @gc
	string toString()
	{		
		return to!string(c);
	}

	//compile-time
	private static string emitXYZMembers()
	{
		string code;
		string[] n = ["x","y","z","w"];
		foreach(i; 0 .. sz)
		{
			if(i >= n.length) break;
			code ~= format("@property ref T %s() { return c[%d]; };\n", n[i], i);
		}		
		return code;
	}
}

unittest
{
	{
		auto v1 = Vector!(2, float)(0,0);
		auto v2 = Vector!(2, float)(3,4);
		v1.x = 1;
		v1.y = 2;
		assert(v1 + v2 == Vector!(2, float)(4,6));
	}

	{
		auto v1 = Vector!(3, double)(0,0,0);
		auto v2 = Vector!(3, double)(1,0,1);
		v1.y = 1;
		v2.z = 2;
		assert(v1 - v2 == Vector!(3, double)(-1, 1, -2));		
	}		
}