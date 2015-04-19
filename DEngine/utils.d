module utils;
import std.format;
import std.array;

@safe:

wstring wformat(A...)(wstring fmt, A args)
{
	auto a = appender!wstring;
	a.formattedWrite(fmt, args);
	return a.data;
}