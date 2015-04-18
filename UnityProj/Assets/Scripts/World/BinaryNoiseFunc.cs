using UnityEngine;

namespace Engine
{
    public class BinaryNoiseFunc
    {
        public Vector2 randomOffset;
        public float frequency;
        public float perlinThreshold;

        public BinaryNoiseFunc(Vector2 randomOffset, float frequency, float threshold)
        {            
            this.randomOffset = randomOffset;
            this.frequency = frequency;
            this.perlinThreshold = 1 - threshold * 2;
        }

        public bool Get(Vector2 p)
        {
            float noiseVal = Noise.Perlin2D(p + randomOffset, frequency);
            return noiseVal > perlinThreshold;
        }
    }
}

