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
    

	protected override void OnEnable()
    {
        base.OnEnable();
        light = transform.GetChild(0).GetChild(0).GetComponent<Light>();
        light.intensity = 0;     

        coreParticles = transform.GetChild(0).GetChild(1).GetComponent<ParticleSystem>();
        coreRenderer = transform.GetChild(0).GetChild(1).GetComponent<ParticleSystemRenderer>();
        cinderParticles = transform.GetChild(0).GetChild(2).GetComponent<ParticleSystem>();

        origCoreSpeed = coreParticles.startSpeed;
        origTint = coreRenderer.sharedMaterial.GetColor("_TintColor");
        origCinderEmitRate = cinderParticles.emissionRate;
    }
	
	
	void Update()
    {
        if (deathTimeLeft.HasValue)
        {
            deathTimeLeft -= Time.deltaTime;
            if (deathTimeLeft <= 0)
            {
                Destroy(coreRenderer.material);
                Destroy(gameObject);
            }
            else
            {
                light.intensity *= deathTimeLeft.Value / deathTime;
            }
        }

        if (power.IsNew)
        {
            coreParticles.startSpeed = Mathf.Lerp(0.25f, 1f, power.value) * origCoreSpeed;
            Color c = origTint;
            c = new Color(c.r, c.g, c.b, Mathf.Lerp(0.02f, 1f, power.value) * c.a);

            //Instantiate in editor only if playing not editing
            if(G.IsEditing())
                coreRenderer.sharedMaterial.SetColor("_TintColor", c);
            else            
                coreRenderer.material.SetColor("_TintColor", c);           


            cinderParticles.emissionRate = Mathf.Lerp(0, 1f, power.value) * origCinderEmitRate;        
        }

        light.intensity = Mathf.Lerp(0.1f, 1f, power.value) * Random.Range(lightIntensity - lightIntensity * lightFlickerPart, lightIntensity);
    }

    public override void Die()
    {        
        deathTimeLeft = deathTime;
        coreParticles.emissionRate = 0;
        cinderParticles.emissionRate = 0;
    }

    
}
