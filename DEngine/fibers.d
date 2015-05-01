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

void startFiber(void function() fn)
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

unittest
{
	foreach(k; 0..1000)
	{
		startFiber(() 
			   { 
				   //writeln("Something start");
				   foreach(i; 0..5)
				   {
					   delay(2);
					   //writeln("Something loop: " ~ to!string(i));
				   }
			   });

		float time = 0;
		immutable float dt = 0.5;
		foreach(i; 0..50)
		{
			//writeln("time " ~ to!string(time));
			time += dt;
			update(dt);
		}
	}	
}


