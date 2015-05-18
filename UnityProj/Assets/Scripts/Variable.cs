using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public class FloatVariable
{
    [SerializeField]
    float value;

    bool isNew;

    public FloatVariable(float v)
    {
        value = v;
        isNew = true;
    }

    public float Value
    {
        get
        {
            return value;
        }
        set
        {
            if (!this.value.Equals(value))
            {
                isNew = true;
                this.value = value;
            }
        }
    }

    public bool IsNew
    {
        get
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying) //Field editing in inspector updates field, not property, so everything should update every frame
                return true;
#endif
            bool n = isNew;
            isNew = false;
            return n;
        }
    }

   
}



