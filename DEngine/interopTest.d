module interopTest;
import std.random;
import std.conv;
import engine;
import utils;

extern(C) void function(wchar*) externLog;

extern(C) export void start()
{
	wb1 = new WorldBlock!10(HexXY(2,3));
	wb1.generate(BinaryNoiseFunc(Vector2(100, 200), 0.25f, 0.6f), 
				 BinaryNoiseFunc(Vector2(200, 100), 0.25f, 0.4f));

	log(wformat("Non empty: %d", wb1.nonEmptyCellsCount));
}

WorldBlock!10 wb1;
extern(C) export void* getWorldBlockHandle()
{
	return cast(void*)wb1 + wb1.position.offsetof;
}



void log(wstring msg)
{	
	externLog(cast(wchar*)msg);
}

extern(C) export int sqr(int x)
{
	log("sqr called!"w);
	return x * x; 
}

extern(C) export int dbl(int x)
{
	log("dbl called"w);
	return 2 * x;
}

extern(C) export int five()
{
	log("five called"w);
	return 5;
}

extern(C) export void setLogger(void function(wchar*) logPtr)
{
	externLog = logPtr;
}



