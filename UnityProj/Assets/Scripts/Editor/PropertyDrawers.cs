using UnityEngine;
using System.Collections;
using UnityEditor;
using Engine;
using System.Reflection;
using System;

[CustomPropertyDrawer(typeof(VariableFloat))]
public class VariableFloatPropertyDrawer : UnityEditor.PropertyDrawer
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

[CustomPropertyDrawer(typeof(ScriptObjectID))]
public class ScriptObjectIDPropertyDrawer : UnityEditor.PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
#if UNITY_EDITOR
        if (LevelEditor.S != null && LevelEditor.S.levelScriptIDs != null)
        {
            EditorGUI.BeginProperty(position, label, property);

            var valueProp = property.FindPropertyRelative("id");
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);            
            Enum result = EditorGUI.EnumPopup(position, (Enum)Enum.ToObject(LevelEditor.S.levelScriptIDs, valueProp.intValue));
            valueProp.intValue = (int)Convert.ChangeType(result, typeof(int));

            EditorGUI.EndProperty();
#endif
        }
    }
}