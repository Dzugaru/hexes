module main;

import std.stdio;
import engine;
import math;

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
	
	writeln("Hello world!");
	
	auto a = Vector2(1,1).normalized;
	writeln(a);
	readln();
}