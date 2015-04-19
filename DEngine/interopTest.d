module interopTest;

extern(C) export int sqr(int x)
{
	return x * x;
}

extern(C) export int dbl(int x)
{
	return 2 * x;
}

extern(C) export int five()
{
	return 5;
}