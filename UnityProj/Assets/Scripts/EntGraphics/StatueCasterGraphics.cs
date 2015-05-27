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
            return entity.GetType() == typeof(StatueCaster) ||
                   (entity.GetType() == typeof(StatueTeach) && !isCasting.value);
        }
    }

    public StatueCasterGraphics()
    {
       
    }      

    void Start()
    {       
        isCasting = new VariableBool(() => ((Statue)entity).isCasting);

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
    public MechType type;

    public override Entity CreateEntity()
    {
        Statue statue;
        switch (type)
        {
            case MechType.StatueCaster: statue = new StatueCaster(); break;
            case MechType.StatueTeachMelee: statue = new StatueTeach(); break;
            default: throw new Tools.AssertException();
        }

        statue.entityType = (uint)type;
        statue.dir = 0;
        statue.sourceSpellPos = sourceSpellPos;
        return statue;
    }

    void EditorStart()
    {
        sourceSpellPos = ((Statue)entity).sourceSpellPos;        
    }

    void EditorUpdate()
    {
        ((Statue)entity).sourceSpellPos = sourceSpellPos;        
    }
#endif
}

