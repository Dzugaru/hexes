module main;

import std.stdio;
import engine;
import math;
import logger;
import std.datetime;
import std.random;
import frontendMock;
import enums;

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
	
	engine.startTheWorld();

	//auto mob1 = Mob.allocate(GrObjType.Cube, 1.0);
	//mob1.spawn(HexXY(0,0));
	//mob1.setDest(HexXY(2,2));

	foreach(i; 0..100)
	{
		engine.update(0.1);
	}


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


