module logger;

enum isLogging = true;

void function(wstring msg) logImpl;

@trusted void log(lazy wstring msg)
{
	if(isLogging && logImpl)
	{
		logImpl(msg());
	}
}


