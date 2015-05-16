using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Flame : SpellEffectGraphics
{
    new Light light;
    ParticleSystem coreParticles, cinderParticles;
    ParticleSystemRenderer coreRenderer;

    float origCoreSpeed, origCinderEmitRate;
    Color origTint;

    public float lightIntensity = 2.5f;
    public float lightFlickerPart = 0.2f;
    

	void OnEnable()
    {
        power.value = 1;

        light = transform.GetChild(0).GetComponent<Light>();        

        coreParticles = transform.GetChild(1).GetComponent<ParticleSystem>();
        coreRenderer = transform.GetChild(1).GetComponent<ParticleSystemRenderer>();
        cinderParticles = transform.GetChild(2).GetComponent<ParticleSystem>();

        origCoreSpeed = coreParticles.startSpeed;
        origTint = coreRenderer.sharedMaterial.GetColor("_TintColor");
        origCinderEmitRate = cinderParticles.emissionRate;
    }
	
	
	void Update()
    {          
        light.intensity = Mathf.Lerp(0.1f, 1f, power.value) * Random.Range(lightIntensity - lightIntensity * lightFlickerPart, lightIntensity);

        if (power.isNew)
        {            
            coreParticles.startSpeed = Mathf.Lerp(0.25f, 1f, power.value) * origCoreSpeed;
            Color c = origTint;
            c = new Color(c.r, c.g, c.b, Mathf.Lerp(0.02f, 1f, power.value) * c.a);

            //Instantiate in editor only if playing not editing
#if UNITY_EDITOR
            if(UnityEditor.EditorApplication.isPlaying)
                coreRenderer.material.SetColor("_TintColor", c);
            else
#endif
            coreRenderer.sharedMaterial.SetColor("_TintColor", c);

            cinderParticles.emissionRate = Mathf.Lerp(0, 1f, power.value) * origCinderEmitRate;        
        }
	}
}
