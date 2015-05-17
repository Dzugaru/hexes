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
    public float deathTime = 1;

    float? deathTimeLeft;
    

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

        if (deathTimeLeft.HasValue)
        {
            deathTimeLeft -= Time.deltaTime;
            if (deathTimeLeft <= 0) Destroy(gameObject);
            else
            {
                light.intensity *= deathTimeLeft.Value / deathTime;
            }
        }

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
                coreRenderer.sharedMaterial.SetColor("_TintColor", c);
#else
            coreRenderer.material.SetColor("_TintColor", c);
#endif


            cinderParticles.emissionRate = Mathf.Lerp(0, 1f, power.value) * origCinderEmitRate;        
        }
	}

    public override void Die()
    {
        deathTimeLeft = deathTime;
        coreParticles.emissionRate = 0;
        cinderParticles.emissionRate = 0;
    }

    
}
