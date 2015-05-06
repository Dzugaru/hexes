/**
* Interfacing with Unity3D graphics engine
*/

module interfacing;
import engine;
import logger;
import std.string;
import std.conv : to;
import std.traits;
import enums;
import std.array : replaceFirst;
import std.string : startsWith;
import core.exception;

/**
* Callbacks wrapper
*/
class cb
{
	@disable this();
static:
private:
	immutable string[] decls = [
		q{void function(immutable(char)*)},														q{log},
		q{void function(HexXY pos, EffectType effect)},											q{showEffectOnTile},
		q{GrObjHandle function(GrObjClass objClass, GrObjType objType)},						q{createGrObj},
		q{void function(GrObjHandle objHandle, GrObjOperation op, void* opParams)},				q{performOpOnGrObj},		
	];

	string emitDecls()
	{
		string code;
		//Aliases are needed for cast from void* in setCallback	
		foreach(i; 0 .. decls.length / 2)
		{
			string type = decls[i*2];
			string typeAlias = "T" ~ decls[i*2 + 1];
			string name = decls[i*2 + 1];
			code ~= q{ alias extern(C) } ~ type ~ " " ~ typeAlias  ~ "; " ~ typeAlias ~ " " ~ name ~ ";\n";
		}
		
		return code;		
	}

	//Emits realizations to void returning callbacks that do nothing
	string emitBlackholes()
	{
		string code;		
		foreach(i; 0 .. decls.length / 2)
		{
			string type = decls[i*2];
			if(!type.startsWith("void")) continue;
			string name = decls[i*2 + 1];
			code ~= "extern (C) " ~ type.replaceFirst("function", "_BH" ~ name) ~ "{}\n";
		}		
		return code;
	}



	mixin(emitBlackholes());
	
public:
	mixin(emitDecls());

	void setBlackholes()
	{
		static string emitSetBlackholes()
		{
			string code;		
			foreach(i; 0 .. decls.length / 2)
			{			
				string type = decls[i*2];
				if(!type.startsWith("void")) continue;
				string name = decls[i*2 + 1];
				code ~= name ~ " = &_BH" ~ name ~ ";\n";
			}		
			return code;
		}

		mixin(emitSetBlackholes());
	}
}

/**
* Gets called by Unity3D immediately after game start
*/
extern(C) export void onStart()
{
	//Starting
	engine.startTheWorld();
}

bool updatesDisabledDueToError = false;

/**
* Gets called by Unity3D every frame
*/
extern(C) export void update(float dt)
{			
	if(updatesDisabledDueToError) return;
	try
	{
		engine.update(dt);
	}
	catch(Throwable th)
	{
		log(format("Exception thrown: %s", th));
		updatesDisabledDueToError = true;
	}	
}

extern(C) export void setLogging(void function(immutable(char)*) l)
{
	cb.log = l;
	static void logAdapt(string msg)
	{
		cb.log(toStringz(msg));
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
	//log("Callback try set: " ~ to!string(name));	

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

	//pragma(msg, emitSwitch());	
}

/**
* Graphics object handle
*/
struct GrObjHandle
{
align:
	GrObjClass objClass;
	uint idx;
}

//UI methods
extern(C) export void playerMove(HexXY p)
{
	try
	{
		if(worldBlock.cellType(p) != TerrainCellType.Empty &&
		   (player.dest.isNull() || p != player.dest))
		{
			player.setDest(p, 10, false);
		}
	}
	catch(Throwable th)
	{
		log(format("Exception thrown: %s", th));
		updatesDisabledDueToError = true;
	}		
}

extern(C) export void playerCast(HexXY p)
{
	try
	{
		player.castSpell(p);
	}
	catch(Throwable th)
	{
		log(format("Exception thrown: %s", th));
		updatesDisabledDueToError = true;
	}	
}





