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

/***************************
* Single-linked list, that uses unique existing field "nextEl" in an object to link,
* therefore avoiding any additional memory allocations
*/
struct SLList(T, alias nextEl)
if(is(T == class))
{
	T head;
	static immutable string next = __traits(identifier, nextEl);

	void insert(T el)
	{		
		if(head is null)		
			head = el;
		else
		{
			mixin(q{el.} ~ next) = head;
			head = el;
		}		
	}

	void remove(T el)
	{	
		if(head is el) head = mixin(q{head.} ~ next);
		else
		{
			T curr = head, prev;
			do 
			{ 
				prev = curr;
				curr = mixin(q{curr.} ~ next);
			} while(curr !is el);
			mixin(q{prev.} ~ next) = mixin(q{curr.} ~ next);
		}
	}

	Iter els()
	{
		return Iter(head);
	}

	struct Iter
	{
		T c;

		this(T c)
		{
			this.c = c;
		}

		@trusted int opApply(int delegate(T) loopBody)
		{
			int result = 0;
			while(c !is null)
			{
				result = loopBody(c);
				if (result)	break;
				c = mixin(q{c.} ~ next);
			}
			return result;
		}	
	}
}

unittest
{
	class Foo
	{
		int x;
		this(int x)
		{
			this.x = x;
		}

		Foo listNextEl;
	}

	auto list = SLList!(Foo, Foo.listNextEl)();

	list.insert(new Foo(1));
	auto f2 = new Foo(2);
	list.insert(f2);
	list.insert(new Foo(3));
	list.remove(f2);
	int[] vals;
	
	foreach(e; list.els())	
		vals ~= e.x;
	assert(vals == [3,1]);

	list.insert(new Foo(4));
	vals.length = 0;
	foreach(e; list.els())	
		vals ~= e.x;
	assert(vals == [4,3,1]);
}

