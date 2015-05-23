using UnityEngine;
using Engine;
using System.Collections;

[ExecuteInEditMode]
public class SpellEffectGraphics : EntityGraphics
{
    public float scale = 1;
    public VariableFloat power;

    protected virtual void OnEnable()
    {
#if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isPlaying)
            power = new VariableFloat(() => ((SpellEffect)entity).power);
        else
            power = new VariableFloat() { value = 1 };
#else
        power = new VariableFloat(() => ((SpellEffect)entity).power);
#endif  

        ScaleParticleSystems();        
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
