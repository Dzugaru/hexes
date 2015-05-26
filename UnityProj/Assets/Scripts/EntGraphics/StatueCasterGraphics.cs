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

    public bool CanBeHighlighted
    {
        get
        {
            var behType = ((StatueCaster)entity).behType;
            return behType == StatueCaster.BehaviorType.CastingSpell ||
                   (behType == StatueCaster.BehaviorType.TeachingMeleeSpell && !isCasting.value);
        }
    }

    public StatueCasterGraphics()
    {
       
    }      

    void Start()
    {       
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
    public StatueCaster.BehaviorType behType;

    public override Entity CreateEntity()
    {
        return new StatueCaster(behType) { dir = 0, sourceSpellPos = sourceSpellPos };
    }

    void EditorStart()
    {
        sourceSpellPos = ((StatueCaster)entity).sourceSpellPos;
        behType = ((StatueCaster)entity).behType;
    }

    void EditorUpdate()
    {
        ((StatueCaster)entity).sourceSpellPos = sourceSpellPos;
        ((StatueCaster)entity).behType = behType;
    }
#endif
}

