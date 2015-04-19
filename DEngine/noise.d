module noise;
import std.math;
import math;

@safe:
@nogc:
nothrow:

pure private float smooth(float t)
{
	return t * t * t * (t * (t * 6f - 15f) + 10f);
}

pure float perlin1D(float point, float frequency)
{
	point *= frequency;
	int i0 = cast(int)floor(point);
	float t0 = point - i0;
	float t1 = t0 - 1f;
	i0 &= hashMask;
	int i1 = i0 + 1;

	float g0 = gradients1D[hash[i0] & gradientsMask1D];
	float g1 = gradients1D[hash[i1] & gradientsMask1D];

	float v0 = g0 * t0;
	float v1 = g1 * t1;

	float t = smooth(t0);

	float a = v0;
	float b = v1 - v0;                

	return (a + b * t) * 2f; 
}

pure float perlin2D(Vector2 point, float frequency)
{
	point *= frequency;
	int ix0 = cast(int)floor(point.x);
	int iy0 = cast(int)floor(point.y);
	float tx0 = point.x - ix0;
	float ty0 = point.y - iy0;
	float tx1 = tx0 - 1f;
	float ty1 = ty0 - 1f;
	ix0 &= hashMask;
	iy0 &= hashMask;
	int ix1 = ix0 + 1;
	int iy1 = iy0 + 1;

	int h0 = hash[ix0];
	int h1 = hash[ix1];
	Vector2 g00 = gradients2D[hash[h0 + iy0] & gradientsMask2D];
	Vector2 g10 = gradients2D[hash[h1 + iy0] & gradientsMask2D];
	Vector2 g01 = gradients2D[hash[h0 + iy1] & gradientsMask2D];
	Vector2 g11 = gradients2D[hash[h1 + iy1] & gradientsMask2D];

	float v00 = g00.dot(Vector2(tx0, ty0));
	float v10 = g10.dot(Vector2(tx1, ty0));
	float v01 = g01.dot(Vector2(tx0, ty1));
	float v11 = g11.dot(Vector2(tx1, ty1));

	float tx = smooth(tx0);
	float ty = smooth(ty0);

	float a = v00;
	float b = v10 - v00;
	float c = v01 - v00;
	float d = v11 - v01 - v10 + v00; 

	return (a + b * tx + (c + d * tx) * ty) * sqrt(2f);
}

pure float perlin3D(Vector3 point, float frequency)
{
	point *= frequency;
	int ix0 = cast(int)floor(point.x);
	int iy0 = cast(int)floor(point.y);
	int iz0 = cast(int)floor(point.z);
	float tx0 = point.x - ix0;
	float ty0 = point.y - iy0;
	float tz0 = point.z - iz0;
	float tx1 = tx0 - 1f;
	float ty1 = ty0 - 1f;
	float tz1 = tz0 - 1f;
	ix0 &= hashMask;
	iy0 &= hashMask;
	iz0 &= hashMask;
	int ix1 = ix0 + 1;
	int iy1 = iy0 + 1;
	int iz1 = iz0 + 1;

	int h0 = hash[ix0];
	int h1 = hash[ix1];
	int h00 = hash[h0 + iy0];
	int h10 = hash[h1 + iy0];
	int h01 = hash[h0 + iy1];
	int h11 = hash[h1 + iy1];
	Vector3 g000 = gradients3D[hash[h00 + iz0] & gradientsMask3D];
	Vector3 g100 = gradients3D[hash[h10 + iz0] & gradientsMask3D];
	Vector3 g010 = gradients3D[hash[h01 + iz0] & gradientsMask3D];
	Vector3 g110 = gradients3D[hash[h11 + iz0] & gradientsMask3D];
	Vector3 g001 = gradients3D[hash[h00 + iz1] & gradientsMask3D];
	Vector3 g101 = gradients3D[hash[h10 + iz1] & gradientsMask3D];
	Vector3 g011 = gradients3D[hash[h01 + iz1] & gradientsMask3D];
	Vector3 g111 = gradients3D[hash[h11 + iz1] & gradientsMask3D];

	float v000 = g000.dot(Vector3(tx0, ty0, tz0));
	float v100 = g100.dot(Vector3(tx1, ty0, tz0));
	float v010 = g010.dot(Vector3(tx0, ty1, tz0));
	float v110 = g110.dot(Vector3(tx1, ty1, tz0));
	float v001 = g001.dot(Vector3(tx0, ty0, tz1));
	float v101 = g101.dot(Vector3(tx1, ty0, tz1));
	float v011 = g011.dot(Vector3(tx0, ty1, tz1));
	float v111 = g111.dot(Vector3(tx1, ty1, tz1));

	float tx = smooth(tx0);
	float ty = smooth(ty0);
	float tz = smooth(tz0);

	float a = v000;
	float b = v100 - v000;
	float c = v010 - v000;
	float d = v001 - v000;
	float e = v110 - v010 - v100 + v000;
	float f = v101 - v001 - v100 + v000;
	float g = v011 - v001 - v010 + v000;
	float h = v111 - v011 - v101 + v001 - v110 + v010 + v100 - v000;

	return a + b * tx + (c + e * tx) * ty + (d + f * tx + (g + h * tx) * ty) * tz;
}

private immutable float[] gradients1D = [1f, -1f];

private immutable int gradientsMask1D = 1;

private immutable Vector2[] gradients2D = [
	Vector2(1f, 0f),
	Vector2(-1f, 0f),
	Vector2( 0f, 1f),
	Vector2( 0f,-1f),
	Vector2( 1f, 1f).normalized,
	Vector2(-1f, 1f).normalized,
	Vector2( 1f,-1f).normalized,
	Vector2(-1f,-1f).normalized
];

private immutable int gradientsMask2D = 7;

private immutable Vector3[] gradients3D = [
    Vector3( 1f, 1f, 0f),
    Vector3(-1f, 1f, 0f),
    Vector3( 1f,-1f, 0f),
    Vector3(-1f,-1f, 0f),
    Vector3( 1f, 0f, 1f),
    Vector3(-1f, 0f, 1f),
    Vector3( 1f, 0f,-1f),
    Vector3(-1f, 0f,-1f),
    Vector3( 0f, 1f, 1f),
    Vector3( 0f,-1f, 1f),
    Vector3( 0f, 1f,-1f),
    Vector3( 0f,-1f,-1f),
    Vector3( 1f, 1f, 0f),
    Vector3(-1f, 1f, 0f),
    Vector3( 0f,-1f, 1f),
    Vector3( 0f,-1f,-1f)
];

private immutable int gradientsMask3D = 15;

private immutable uint hashMask = 255;

private immutable uint[] hash = [
	151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
	140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
	247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
	57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
	74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
	60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
	65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
	200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
	52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
	207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
	119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
	129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
	218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
	81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
	184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
	222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180,

	151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
	140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
	247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
	57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
	74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
	60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
	65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
	200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
	52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
	207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
	119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
	129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
	218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
	81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
	184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
	222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180
];
