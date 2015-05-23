using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using Engine;

public class StatueCasterGraphics : InanimateGraphics, IHighlightable, IClickable
{
    public VariableBool isCasting;

    public StatueCasterGraphics()
    {
       
    }   

    void OnEnable()
    {
        if (G.IsEditing())
            isCasting = new VariableBool() { value = false };
        else
            isCasting = new VariableBool(() => ((StatueCaster)entity).isCasting);
    }

    void Update()
    {
        if (isCasting.IsNew)
        {            
            transform.Find("Casting").gameObject.SetActive(isCasting.value);            
        }
    }
}

