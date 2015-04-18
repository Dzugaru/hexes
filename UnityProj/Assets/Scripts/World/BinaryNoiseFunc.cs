using UnityEngine;

namespace Engine
{
    public class BinaryNoiseFunc
    {
        public Vector2 randomOffset;
        public float frequency;
        public float threshold;

        public BinaryNoiseFunc(Vector2 randomOffset, float frequency, float threshold)
        {
            this.randomOffset = randomOffset;
            this.frequency = frequency;
            this.threshold = threshold;
        }

        public bool Get(Vector2 p)
        {
            return Noise.Perlin2D(p + randomOffset, frequency) > threshold;
        }
    }
}

