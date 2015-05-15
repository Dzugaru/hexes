using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public class FloatVariable
{
    [SerializeField]
    float _value;

    bool _isNew;

    public FloatVariable(float v)
    {
        _value = v;
        _isNew = true;
    }

    public float value
    {
        get
        {
            return _value;
        }
        set
        {
            if (!_value.Equals(value))
            {
                _isNew = true;
                _value = value;
            }
        }
    }

    public bool isNew
    {
        get
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying) //Field editing in inspector updates field, not property, so everything should update every frame
                return true;
#endif
            bool n = _isNew;
            _isNew = false;
            return n;
        }
    }

   
}



