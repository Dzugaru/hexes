/**
* Delayable fibers engine, similar to Unity3D coroutines
* TODO: wait for condition?
*/

module fibers;

public import core.thread : Fiber;
import std.stdio;
import freelist;
public import utils;
public import std.traits : isAssignable;
import logger;

SLList!(FreeFiber, FreeFiber.listNext) freeFibers;

void updateFibersInList(T)(ref T fibList, float dt)
{		
	foreach(fib; fibList.els())
	{
		if(fib.state == Fiber.State.TERM)
		{			
			fibList.remove(fib);
			fib.deallocate();
		}	
		else
		{
			if(fib.delayLeft > 0)		
				fib.delayLeft -= dt;		

			if(fib.delayLeft <= 0)
				fib.call();
		}
	}
}

void updateFree(float dt)
{
	updateFibersInList(freeFibers, dt);
}

void startFree(void function() fn)
{
	auto fiber = FreeFiber.allocate(fn);	
	freeFibers.insert(fiber);
	fiber.call();
}

void startFree(void delegate() fn)
{
	auto fiber = FreeFiber.allocate(fn);	
	freeFibers.insert(fiber);
	fiber.call();
}

abstract class DelayFiber : Fiber
{
	float delayLeft;

	this()
	{
		super((){});
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
	
	void construct(void function() func)
    {
		this.delayLeft = 0;		
		reset(func);
    }	

	void construct(void delegate() del)
    {
		this.delayLeft = 0;		
		reset(del);
    }	
}

class BoundFiber(T) : DelayFiber
{
	mixin Freelist!false;
	BoundFiber!T listNext;
	T context;	

	void construct(T context, void function() func)
    {		
		this.context = context;
		this.delayLeft = 0;				
		reset(func);		
    }	

	void construct(T context, void delegate() del)
    {
		this.context = context;
		this.delayLeft = 0;				
		reset(del);		
    }	
}

string fibDelayWithoutBreak(string delayExpr)()
{
	return q{(cast(DelayFiber)Fiber.getThis()).delay(} ~ delayExpr ~ q{); };
}

string fibDelay(string delayExpr)()
{
	return fibDelayWithoutBreak!delayExpr ~ q{ if(fibIsDestroyed) return; };
}

interface Fibered {}

mixin template _BoundFibers()
{
	static assert(isAssignable!(Fibered, typeof(this)));

	alias TFiber = BoundFiber!(typeof(this));
	SLList!(TFiber, TFiber.listNext) fibList;	

	bool fibIsDestroyed;		

	@property static typeof(this) fibCtx()
	{
		return (cast(TFiber)Fiber.getThis()).context;
	}	

	void startFiber(void function() fn)
	{
		auto fiber = TFiber.allocate(this, fn);	
		fibList.insert(fiber);	
		fiber.call();		
	}

	void startFiber(void delegate() del)
	{
		auto fiber = TFiber.allocate(this, del);	
		fibList.insert(fiber);		
		fiber.call();		
	}

	void updateFibers(float dt)
	{
		updateFibersInList(fibList, dt);
	}

	void deallocateRunningFibers()
	{		
		fibIsDestroyed = true;
		foreach(fib; fibList.els())	
		{			
			fib.call();
			fibList.remove(fib);
			if(fib.state != Fiber.State.TERM)
				logger.log("fiber leak!");			
			else			
				fib.deallocate();	
		}		
	}
}

