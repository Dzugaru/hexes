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

    public uint dir;

    public RuneGraphics()
    {
        
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
        //float colorModifier = 1 + Mathf.Max(0, info.power - 0.75f);
        //float alphaModifier = 0.25f + Mathf.Max(0, info.power - 0.25f);
        //mat.color = new Color(origColor.r * colorModifier, origColor.g * colorModifier, origColor.b * colorModifier, origColor.a * alphaModifier);

        transform.rotation = Quaternion.AngleAxis(dir * 60, new Vector3(0, 1, 0));
    }    
        
   
    

    public void UpdateInterface(uint dir)
    {
        this.dir = dir;
        //float colorModifier = 1 + Mathf.Max(0, power - 0.75f);
    //    //float alphaModifier = 0.25f + Mathf.Max(0, power - 0.25f);
    //   // mat.color = new Color(origColor.r * colorModifier, origColor.g * colorModifier, origColor.b * colorModifier, origColor.a * alphaModifier);
    //    //Debug.Log(power);      

    }
}

