module main;

import std.stdio;
import engine;
import math;
import logger;
import std.datetime;
import std.random;
import frontendMock;
import enums;
import fibers;

//struct Boo
//{
//    private int[1] c;		
//    this(int[] x)
//    {	
//        c[] = x[];
//    }	
//}
//
//immutable Boo a = Boo([1]);

void main()
{
	frontendMock.setup();
	
	//engine.startTheWorld();

	

	//foreach(i; 0..100)
	//{
	//	engine.update(0.1);
	//}

	logger.logImpl = (msg) { writeln(msg); };

	startTheWorld();
	interfacing.playerDrawOrEraseRune(RuneType.Compile, HexXY(1,0), false);
	

	//FiberLeakTest();

	writeln("All ok!");
	readln();
}

//void generateBench()
//{
//    wb.generate(BinaryNoiseFunc(Vector2(uniform(100.,200.), uniform(100.,200.)), 0.25f, 0.6f), 
//                BinaryNoiseFunc(Vector2(uniform(100.,200.), uniform(100.,200.)), 0.25f, 0.4f));
//}
//
//auto dur = benchmark!generateBench(100);
//writeln(dur[0].length / cast(double)dur[0].ticksPerSec);

class A : Fibered
{
	mixin Freelist;
	mixin _BoundFibers;

	void construct() {}
}

void FiberLeakTest()
{
	
	writeln("News: " ~ to!string(freelist.newCount));
	foreach(i; 0..1000000)
	{		
		A a = A.allocate();
		foreach(j; 0 .. 5)
		{
			a.startFiber(()
			{
				with(A.fibCtx)
				{
					//mixin(fibBreak);
					//writeln("fib enter");
					//scope(exit) writeln("fib exit");
					int k = 0;					
					//++counter;

					for(;;)
					{
						++k;
						mixin(fibDelay!q{1});
					}
				}
				
			});
		}
		a.fiberedUpdate(1);


		a.fiberedDie();
		a.deallocate();

		//writeln("Freelist fibers: " ~ to!string(A.TFiber._flCount));
	}

	writeln("News: " ~ to!string(freelist.newCount));
}


