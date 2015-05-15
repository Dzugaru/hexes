using System;
using System.Collections.Generic;
using UnityEngine;

class GObjFreelist : MonoBehaviour
{
    public static GObjFreelist I { get; private set; }

    Dictionary<string, Stack<GameObject>> listsByPrefabName = new Dictionary<string, Stack<GameObject>>();

    public GObjFreelist()
    {
        I = this;
    }

    public GameObject Get(string prefabName)
    {
        Stack<GameObject> list = null;
        if(!listsByPrefabName.TryGetValue(prefabName, out list))
        {
            list = new Stack<GameObject>();
            listsByPrefabName.Add(prefabName, list);
        }

        GameObject obj;
        if (list.Count > 0)
        {
            obj = list.Pop();            
            obj.transform.SetParent(null, false);
        }
        else
        {
            obj = (GameObject)Instantiate(Resources.Load(prefabName));           
        }

        return obj;
    }

    public void Put(GameObject obj, string prefabName)
    {
        Stack<GameObject> list = listsByPrefabName[prefabName];
        list.Push(obj);        
        obj.transform.SetParent(transform, false);
    }
}

