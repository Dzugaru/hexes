module main;

import std.stdio;
import engine;
import math;
import logger;
import std.datetime;
import std.random;

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
	logImpl = (msg) => writeln(msg);
	
	writeln("Hello world!");
	auto wb = new WorldBlock!100(HexXY(0,0));

	void generateBench()
	{
		wb.generate(BinaryNoiseFunc(Vector2(uniform(100.,200.), uniform(100.,200.)), 0.25f, 0.6f), 
					BinaryNoiseFunc(Vector2(uniform(100.,200.), uniform(100.,200.)), 0.25f, 0.4f));
	}

	auto dur = benchmark!generateBench(100);
	writeln(dur[0].length / cast(double)dur[0].ticksPerSec);

	
	
	readln();
}

