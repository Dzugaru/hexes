/**
* Delayable fibers engine, similar to Unity3D coroutines
*/

module fibers;

public import core.thread : Fiber;
import std.stdio;
import freelist;
import utils;

SLList!(FreeFiber, FreeFiber.listNext) freeFibers;

void update(float dt)
{
	foreach(fib; freeFibers.els())
	{
		if(fib.delayLeft > 0)		
			fib.delayLeft -= dt;		

		if(fib.delayLeft <= 0)
			fib.call();

		if(fib.state == Fiber.State.TERM)
		{
			freeFibers.remove(fib);
			fib.deallocate();
		}
	}
}

void start(void function() fn)
{
	auto fiber = FreeFiber.allocate(fn);	
	freeFibers.insert(fiber);
}

void start(T)(T context, void function() fn)
{
	auto fiber = BoundFiber!T.allocate(context, fn);	
	context.fibList.insert(fiber);
}

void delay(float secs)
{
	(cast(DelayFiber)Fiber.getThis()).delay(secs);
}

T getBoundContext(T)()
{
	return (cast(BoundFiber!T)Fiber.getThis()).context;
}

abstract class DelayFiber : Fiber
{
	float delayLeft;

	this(void function() func)
	{
		super(func);
	}

	final void delay(float amount)
	{
		delayLeft += amount;
		Fiber.yield();
	}
}

class FreeFiber : DelayFiber
{
	mixin Freelist!false;
	FreeFiber listNext;	

	void function() func;

	this()
	{
		super(func);
	}
	
	void construct(void function() func)
    {
		this.delayLeft = 0;
		this.func = func;
		reset(func);
    }		
}

class BoundFiber(T) : DelayFiber
{
	mixin Freelist!false;
	BoundFiber!T listNext;
	T context;

	void function() func;
	this()
	{
		super(func);
	}

	void construct(T context, void function() func)
    {
		this.context = context;
		this.delayLeft = 0;
		this.func = func;
		reset(func);
    }	
}

