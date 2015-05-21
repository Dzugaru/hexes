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
}

