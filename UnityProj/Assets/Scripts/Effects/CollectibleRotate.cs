using UnityEngine;
using System.Collections;

public class CollectibleRotate : MonoBehaviour
{
    public float speed;

	void Update ()
    {
        transform.Rotate(Vector3.up, speed * Time.deltaTime / Time.timeScale);
	}
}
