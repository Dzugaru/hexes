/**
* Interfacing with Unity3D graphics engine
*/

module interfacing;
import engine;
import logger;
import std.string;

extern(C) void function(immutable(char)*) loggingCb;

/**
* Gets called by Unity3D immediately after game start
*/
extern(C) export void onStart(void function(immutable(char)*) l)
{
	//Setup logging
	loggingCb = l;
	static void logAdapt(string msg)
	{
		loggingCb(toStringz(msg));
	}
	logger.logImpl = &logAdapt;


	//Starting
	engine.startTheWorld();
}

/**
* Unity3D requests info about the world to draw.
* Just passes a pointer to a single existing WorldBlock for now
*/
extern(C) export void* queryWorld()
{	
	return cast(void*)worldBlock + worldBlock.position.offsetof;
}


