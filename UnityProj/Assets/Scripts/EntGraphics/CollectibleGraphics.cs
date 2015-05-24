using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using Engine;

public class CollectibleGraphics : EntityGraphics, IHighlightable, IClickable
{
    public bool CanBeHighlighted
    {
        get
        {
            return true;
        }
    }

    public CollectibleGraphics()
    {
       
    }      

    void Start()
    {
#if UNITY_EDITOR
        if (LevelEditor.S != null) EditorStart();
#endif
    }

    void Update()
    {
#if UNITY_EDITOR
        if (LevelEditor.S != null) EditorUpdate();
#endif
    }

#if UNITY_EDITOR 
    public string scrollText;
    public CollectibleType type;

    public override Entity CreateEntity()
    {
        switch (type)
        {
            case CollectibleType.Scroll: return new Scroll(scrollText);
            default: throw new Tools.AssertException();
        }        
    }

    void EditorStart()
    {
        Type entType = entity.GetType();
        if (entType == typeof(Scroll))
            scrollText = ((Scroll)entity).text;
    }

    void EditorUpdate()
    {
        switch (type)
        {
            case CollectibleType.Scroll: ((Scroll)entity).text = scrollText; break;            
        }
    }
#endif
}

