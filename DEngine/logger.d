module logger;
import std.stdio;

enum isLogging = true;

void function(string msg) logImpl;

//private File f;
//static this()
//{
//    f = File("log.txt", "w"); 
//}


@trusted void log(lazy string msg)
{
	//f.writeln(msg);
	//f.flush();
	if(isLogging && logImpl)
	{
		logImpl(msg());
	}
}


