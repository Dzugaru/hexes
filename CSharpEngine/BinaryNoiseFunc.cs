using UnityEngine;

namespace Engine
{
    public struct BinaryNoiseFunc
    {       
	    readonly Vector2 randomOffset;
	    readonly float frequency;
        readonly float perlinThreshold;

	    BinaryNoiseFunc(Vector2 randomOffset, float frequency, float threshold)
	    {            
		    this.randomOffset = randomOffset;
		    this.frequency = frequency;
		    this.perlinThreshold = 1 - threshold* 2;
	    }

        public bool Get(Vector2 p)          
	    {
		    float noiseVal = Noise.Perlin2D(p + randomOffset, frequency);
		    return noiseVal > perlinThreshold;
	    }
    }
}
