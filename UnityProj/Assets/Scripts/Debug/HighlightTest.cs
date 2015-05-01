using UnityEngine;
using System.Collections;

public class HighlightTest : MonoBehaviour 
{
    Material hlMat;
    GameObject highlightedCopy;

    public GameObject meshRoot;    
	
	void Start () 
    {        
        hlMat = (Material)Resources.Load("Debug/Highlight");
        highlightedCopy = Instantiate(meshRoot);
        highlightedCopy.transform.parent = meshRoot.transform.parent;
        

        foreach (var mr in highlightedCopy.GetComponentsInChildren<MeshRenderer>())
        {
            //Debug.Log(mr.gameObject.name);

            mr.material = hlMat;
            mr.gameObject.layer = 8;
        }

        foreach (var mr in highlightedCopy.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            //Debug.Log(mr.gameObject.name);
            mr.material = hlMat;
            mr.gameObject.layer = 8;
        }
	}
	
	
	void Update () 
    {
	
	}
}
