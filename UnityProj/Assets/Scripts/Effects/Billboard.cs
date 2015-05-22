using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Billboard : MonoBehaviour
{		
	void Update ()
    {
        Vector3 c = Camera.main.transform.position;
        transform.LookAt(new Vector3(c.x, transform.position.y, c.z), Vector3.up);
    }
}
