using UnityEngine;
using System.Collections;

public class HighlightTest : MonoBehaviour 
{
    //Material hlMat;
    //GameObject highlightedCopy;

    //public GameObject meshRoot;
    public KeyCode key;
    public Color color;
    public float dur, peakAlpha;

	
	void Start () 
    {        
        //hlMat = (Material)Resources.Load("Debug/Highlight");
        //highlightedCopy = Instantiate(meshRoot);
        //highlightedCopy.transform.parent = meshRoot.transform.parent;
        

        //foreach (var mr in highlightedCopy.GetComponentsInChildren<MeshRenderer>())
        //{
        //    //Debug.Log(mr.gameObject.name);

        //    mr.material = hlMat;           
        //}

        //foreach (var mr in highlightedCopy.GetComponentsInChildren<SkinnedMeshRenderer>())
        //{
        //    //Debug.Log(mr.gameObject.name);
        //    mr.material = hlMat;           
        //}
	}
	
	
	void Update () 
    {
        if (Input.GetKeyDown(key))
        {           
            GetComponent<EntityGraphics>().highlight.AnimateFlashHighlight(color, dur, peakAlpha);
        }
	}
}
