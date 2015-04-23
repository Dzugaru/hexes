/**
* Interfacing with Unity3D graphics engine
*/

module interfacing;
import engine;
import logger;
import std.string;
import std.conv : to;
import std.traits;

/**
* Callbacks wrapper
*/
class cb
{
	@disable this();
static:
	//Aliases are needed for cast from void*
	alias extern(C) void function(immutable(char)*) Tlogging;
	Tlogging logging;
	alias extern(C) void function(uint x, uint y, ShowObjectType objType, float durSecs) TshowObjectOnTile;
	TshowObjectOnTile showObjectOnTile;
}

/**
* Gets called by Unity3D immediately after game start
*/
extern(C) export void onStart()
{
	//Starting
	engine.startTheWorld();
}

extern(C) export void calcAndShowPath(int x0, int y0, int x1, int y1)
{
	log("calcAndShowPath called");
	if(cb.showObjectOnTile != null)
	{
		cb.showObjectOnTile(0, 0, ShowObjectType.Sphere, 5);
	}
}

extern(C) export void setLogging(void function(immutable(char)*) l)
{
	cb.logging = l;
	static void logAdapt(string msg)
	{
		cb.logging(toStringz(msg));
	}
	logger.logImpl = &logAdapt;
}

/**
* Unity3D requests info about the world to draw.
* Just passes a pointer to a single existing WorldBlock for now
*/
extern(C) export void* queryWorld()
{	
	return cast(void*)worldBlock + worldBlock.position.offsetof;
}



extern(C) export void setCallback(immutable(char)* name, void* fPtr)
{
	log("Callback try set: " ~ to!string(name));	

	//Emits switch code to cast and assign fPtr according to name
	string emitSwitch()
	{
		string code;		
		foreach(m; __traits(allMembers, cb)) //this is compile-time foreach, cause it is over a TypeTuple
		{		
			//if typeof(...) compiles, this means its an expression (field), not alias of type
			static if(__traits(compiles, typeof(__traits(getMember, cb, m))) && 
					  isFunctionPointer!(__traits(getMember, cb, m)))
			{	
				code ~= "case \"" ~ m ~ "\": cb." ~ m ~ " = cast(cb.T" ~ m ~ ")fPtr; break;\n";				
			}
		}		

		return code;
	}

	switch(to!string(name))
	{
	    mixin(emitSwitch());
	    default: assert(0);
	}

	log(to!string(cb.showObjectOnTile == null));

	pragma(msg, emitSwitch());	
}



