using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;


public class RuneGraphics : InanimateGraphics
{   
    SpriteRenderer renderer;
    Material origMaterial;

    public uint dir;

    public RuneGraphics()
    {
        
    }   

    void Start()
    {
        renderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        origMaterial = renderer.sharedMaterial;
    }

    internal void LearnLight()
    {
       
        renderer.material.SetFloat("_Emission", 2);
    }

    internal void LearnQuench()
    {
        Destroy(renderer.material);
        renderer.sharedMaterial = origMaterial;
        
    }
}

