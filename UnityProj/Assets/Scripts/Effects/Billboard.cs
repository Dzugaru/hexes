using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Billboard : MonoBehaviour
{		
	void Update ()
    {        
        transform.LookAt(Camera.main.transform.position, Vector3.up);
    }
}
