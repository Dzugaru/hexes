using System;
using System.Collections.Generic;
using UnityEngine;

class GObjFreelist : MonoBehaviour
{
    public static GObjFreelist I { get; private set; }

    Dictionary<string, List<GameObject>> listsByPrefabName = new Dictionary<string, List<GameObject>>();

    public GObjFreelist()
    {
        I = this;
    }

    public GameObject Get(string prefabName)
    {
        List<GameObject> list = null;
        if(!listsByPrefabName.TryGetValue(prefabName, out list))
        {
            list = new List<GameObject>();
            listsByPrefabName.Add(prefabName, list);
        }

        GameObject obj;
        if (list.Count > 0)
        {
            obj = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);            
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
        List<GameObject> list = listsByPrefabName[prefabName];
        list.Add(obj);        
        obj.transform.SetParent(transform, false);
    }
}

