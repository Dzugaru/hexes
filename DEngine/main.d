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

	//auto mob1 = Mob.allocate(GrObjType.Cube, 1.0);
	//mob1.spawn(HexXY(0,0));
	//mob1.setDest(HexXY(2,2));

	//foreach(i; 0..100)
	//{
	//	engine.update(0.1);
	//}

	logger.logImpl = (msg) { writeln(msg); };

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
			fibers.start(a, ()
			{
				if(A.fibThis.fibIsDestroyed) return;
				//writeln("fib enter");
				//scope(exit) writeln("fib exit");
				int k = 0;
				if(k > 5) scope(exit) writeln(k);
				
				for(;;)
				{
					++k;
					delay(1); if(A.fibThis.fibIsDestroyed) return;
				}
				
			});
		}
		a.updateFibers(1);


		a.deallocateFibers();
		a.deallocate();

		//writeln("Freelist fibers: " ~ to!string(A.TFiber._flCount));
	}

	writeln("News: " ~ to!string(freelist.newCount));
}


