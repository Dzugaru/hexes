module logger;

enum isLogging = true;

void function(string msg) logImpl;

@trusted void log(lazy string msg)
{
	if(isLogging && logImpl)
	{
		logImpl(msg());
	}
}


