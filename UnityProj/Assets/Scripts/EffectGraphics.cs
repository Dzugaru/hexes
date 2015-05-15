using UnityEngine;
using System.Collections;

//TODO: This should be ObjGraphics too

[ExecuteInEditMode]
public class EffectGraphics : MonoBehaviour
{
    float startTime;

    public float scale = 1;
    public float duration = 1;
    public bool isInfinite = false;

    void Start()
    {
        ScaleParticleSystems();
        startTime = Time.time;
    }

    void Update()
    {
        if (!isInfinite && Time.time - startTime > duration)
            Destroy(gameObject);
    }

    void ScaleParticleSystems()
    {
        ParticleSystem[] particles = gameObject.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem particle in particles)
        {
            particle.startSize *= scale;
            ParticleSystemRenderer renderer = particle.GetComponent<ParticleSystemRenderer>();
            if (renderer)
            {
                renderer.lengthScale *= scale;
                renderer.velocityScale *= scale;                
            }
        }
    }
}
