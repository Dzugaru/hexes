using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class FireLightFlicker : MonoBehaviour
{
    Light light;

	void Start ()
    {
        light = GetComponent<Light>();
	}
	
	
	void Update ()
    {
        light.intensity = Random.Range(0.5f, 0.9f);
	}
}
