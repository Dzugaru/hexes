using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

//NOTE: can't use generics here cause of Unity SerializeField doesn't recognize it

[Serializable]
public class VariableFloat
{
    float prevValue = float.NaN;

    [SerializeField]
    public float value;

    public Func<float> binding;

    public VariableFloat(Func<float> binding = null)
    {
        this.binding = binding;
        if(binding != null)
            value = binding();
    }

    public bool IsNew
    {
        get
        {
            if (binding != null) value = binding();

            bool isNew = value != prevValue;
            prevValue = value;
            return isNew;
        }
    }
}

[Serializable]
public class VariableBool
{
    bool? prevValue = null;

    [SerializeField]
    public bool value;

    public Func<bool> binding;

    public VariableBool(Func<bool> binding = null)
    {
        this.binding = binding;
        if (binding != null)
            value = binding();
    }

    public bool IsNew
    {
        get
        {
            if (binding != null) value = binding();

            bool isNew = value != prevValue;
            prevValue = value;
            return isNew;
        }
    }
}

public class Variable<T> where T : IEquatable<T>
{
    T prevValue = default(T);

    
    public T value;

    public Func<T> binding;

    public Variable(Func<T> binding = null)
    {
        this.binding = binding;
        if (binding != null)
            value = binding();
    }

    public bool IsNew
    {
        get
        {
            if (binding != null) value = binding();

            bool isNew = !value.Equals(prevValue);
            prevValue = value;
            return isNew;
        }
    }
}





