/**
* Delayable fibers engine, similar to Unity3D coroutines
*/

module fibers;

import core.thread;
import std.stdio;
import freelist;
import utils;

SLList!(MyFiber, MyFiber.allFibersListNext) allFibers;

void update(float dt)
{
	foreach(fib; allFibers.els())
	{
		if(fib.delayLeft > 0)		
			fib.delayLeft -= dt;		

		if(fib.delayLeft <= 0)
			fib.call();

		if(fib.state == Fiber.State.TERM)
		{
			allFibers.remove(fib);
			fib.deallocate();
		}
	}
}

void start(void function() fn)
{
	auto fiber = MyFiber.allocate(fn);	
	allFibers.insert(fiber);
}

void delay(float secs)
{
	(cast(MyFiber)Fiber.getThis()).delay(secs);
}

class MyFiber : Fiber
{
	mixin Freelist!false;

	MyFiber allFibersListNext;

	float delayLeft;
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

	void delay(float amount)
	{
		delayLeft += amount;
		Fiber.yield();
	}
}

