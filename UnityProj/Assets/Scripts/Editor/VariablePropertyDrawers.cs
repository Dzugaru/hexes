﻿using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomPropertyDrawer(typeof(VariableFloat))]
public class PropertyDrawer : UnityEditor.PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {        
        EditorGUI.BeginProperty(position, label, property);

        var valueProp = property.FindPropertyRelative("value");
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        valueProp.floatValue = EditorGUI.FloatField(position, valueProp.floatValue);


        EditorGUI.EndProperty();
    }

}