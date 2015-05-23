using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;


public class RuneGraphics : EntityGraphics
{   
    SpriteRenderer renderer;
    Material origMaterial;

    public uint dir;
    public VariableBool isLit;

    void Start()
    {
        if(G.IsInUnityEditMode())
            isLit = new VariableBool() { value = false };
        else
            isLit = new VariableBool(() => ((Rune)entity).isLit);

        renderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        origMaterial = renderer.sharedMaterial;
    }

    void Update()
    {
        if (isLit.IsNew)
        {
            if (isLit.value)
            {
                renderer.material.SetFloat("_Emission", 2);
            }
            else
            {
                Destroy(renderer.material);
                renderer.sharedMaterial = origMaterial;
            }
        }
    }    
}

