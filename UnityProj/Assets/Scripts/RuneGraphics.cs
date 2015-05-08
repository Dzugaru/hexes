using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class RuneGraphics : InanimateGraphics
{
    Color origColor;
    Material mat;
    public float power = 1;

    void Start()
    {
        SpriteRenderer renderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        mat = new Material(renderer.material);
        renderer.material = mat;

        origColor = mat.color;     
    }

    void LateUpdate()
    {        
        float colorModifier = 1 + Mathf.Max(0, power - 0.75f);
        float alphaModifier = 0.25f + Mathf.Max(0, power - 0.25f);
        mat.color = new Color(origColor.r * colorModifier, origColor.g * colorModifier, origColor.b * colorModifier, origColor.a * alphaModifier);
    }

    protected override unsafe void UpdateInfo(void* args)
    {        
        power = *(float*)args;
        //float colorModifier = 1 + Mathf.Max(0, power - 0.75f);
        //float alphaModifier = 0.25f + Mathf.Max(0, power - 0.25f);
       // mat.color = new Color(origColor.r * colorModifier, origColor.g * colorModifier, origColor.b * colorModifier, origColor.a * alphaModifier);
        //Debug.Log(power);      

    }
}

