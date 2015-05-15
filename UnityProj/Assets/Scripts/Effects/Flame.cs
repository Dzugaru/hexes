﻿using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Flame : MonoBehaviour
{
    new Light light;
    ParticleSystem coreParticles, cinderParticles;
    ParticleSystemRenderer coreRenderer;

    float origCoreSpeed, origCinderEmitRate;
    Color origTint;

    public FloatVariable power = new FloatVariable(1);

	void OnEnable()
    {
        power.value = 1;

        light = transform.GetChild(0).GetComponent<Light>();        

        coreParticles = transform.GetChild(1).GetComponent<ParticleSystem>();
        coreRenderer = transform.GetChild(1).GetComponent<ParticleSystemRenderer>();
        cinderParticles = transform.GetChild(2).GetComponent<ParticleSystem>();

        origCoreSpeed = coreParticles.startSpeed;
        //Dont spawn a separate material if we're in edit scene mode
#if !UNITY_EDITOR
        coreRenderer.material = new Material(coreRenderer.sharedMaterial);
#else
        if(UnityEditor.EditorApplication.isPlaying)
            coreRenderer.material = new Material(coreRenderer.sharedMaterial);
#endif
        origTint = coreRenderer.material.GetColor("_TintColor");
        origCinderEmitRate = cinderParticles.emissionRate;
    }
	
	
	void Update()
    {
        light.intensity = Random.Range(0.5f, 0.9f);

        if (power.isNew)
        {
            coreParticles.startSpeed = Mathf.Lerp(0.25f, 1f, power.value) * origCoreSpeed;
            Color c = origTint;
            c = new Color(c.r, c.g, c.b, Mathf.Lerp(0.25f, 1f, power.value) * c.a);
            coreRenderer.material.SetColor("_TintColor", c);
            cinderParticles.emissionRate = Mathf.Lerp(0, 1f, power.value) * origCinderEmitRate;        
        }
	}
}
