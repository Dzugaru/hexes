using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;


public class RuneGraphics : InanimateGraphics
{
    Color origColor;
    Material mat;

    public RuneGraphics()
    {
        info.power = 1;
    }   

    void Start()
    {
        SpriteRenderer renderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        mat = new Material(renderer.material);
        renderer.material = mat;

        origColor = mat.color;     
    }

    void LateUpdate()
    {        
        float colorModifier = 1 + Mathf.Max(0, info.power - 0.75f);
        float alphaModifier = 0.25f + Mathf.Max(0, info.power - 0.25f);
        mat.color = new Color(origColor.r * colorModifier, origColor.g * colorModifier, origColor.b * colorModifier, origColor.a * alphaModifier);

        transform.rotation = Quaternion.AngleAxis(info.dir * 60, new Vector3(0, 1, 0));
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Info
    {
        public float power;
        public uint dir;
    }

    public Info info;

    protected override unsafe void UpdateInfo(void* args)
    {        
        info = *(Info*)args;    
        //float colorModifier = 1 + Mathf.Max(0, power - 0.75f);
        //float alphaModifier = 0.25f + Mathf.Max(0, power - 0.25f);
       // mat.color = new Color(origColor.r * colorModifier, origColor.g * colorModifier, origColor.b * colorModifier, origColor.a * alphaModifier);
        //Debug.Log(power);      

    }
}

