using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using Engine;

public class StatueCasterGraphics : EntityGraphics, IHighlightable, IClickable
{
    public VariableBool isCasting;

    public StatueCasterGraphics()
    {
       
    }      

    void Start()
    {
        if (G.IsInUnityEditMode())
            isCasting = new VariableBool() { value = false };
        else
            isCasting = new VariableBool(() => ((StatueCaster)entity).isCasting);

#if UNITY_EDITOR
        if (LevelEditor.S != null) EditorStart();
#endif
    }

    void Update()
    {
        if (isCasting.IsNew)
        {            
            transform.Find("Casting").gameObject.SetActive(isCasting.value);            
        }

#if UNITY_EDITOR
        if (LevelEditor.S != null) EditorUpdate();
#endif
    }

#if UNITY_EDITOR
    public HexXY sourceSpellPos;

    public override Entity CreateEntity()
    {
        return new StatueCaster(0, sourceSpellPos);
    }

    void EditorStart()
    {
        sourceSpellPos = ((StatueCaster)entity).sourceSpellPos;
    }

    void EditorUpdate()
    {
        ((StatueCaster)entity).sourceSpellPos = sourceSpellPos;
    }
#endif
}

