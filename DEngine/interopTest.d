module interopTest;
import std.random;
import std.conv;

extern(C) void function(wchar*) externLog;

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