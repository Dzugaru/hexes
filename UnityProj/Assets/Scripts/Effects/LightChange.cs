using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class LightChange : MonoBehaviour
{
    new Light light;
    float src, target, progress;

    public float meanIntensity;
    public float varIntensity;
    public float changeTime;

    void OnEnable()
    {
        light = GetComponent<Light>();
    }

    void Update()
    {
        if (progress > 0)
        {
            progress = Mathf.Max(0, progress - Time.deltaTime / changeTime);
            light.intensity = Mathf.SmoothStep(src, target, 1 - progress);
        }
        else
        {
            target = UnityEngine.Random.Range(meanIntensity - varIntensity, meanIntensity + varIntensity);
            if (changeTime == 0)
                light.intensity = target;
            else
            {
                progress = 1;
                src = light.intensity;
            }                         
        }
    }
}

