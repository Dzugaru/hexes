using UnityEngine;
using Engine;
using System.Collections;

[ExecuteInEditMode]
public class SpellEffectGraphics : EntityGraphics
{
    public float scale = 1;    
    public bool isInfinite = false;

    void Start()
    {
        ScaleParticleSystems();        
    }

    void Update()
    {
        
    }

    public override void Spawn(HexXY pos)
    {
        Vector2 planeCoord = pos.ToPlaneCoordinates();
        transform.position = new Vector3(planeCoord.x, 0, planeCoord.y);
        gameObject.SetActive(true);
    }

    public override void Die()
    {
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
