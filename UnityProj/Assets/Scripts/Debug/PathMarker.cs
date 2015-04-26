using UnityEngine;
using System.Collections;

public class PathMarker : MonoBehaviour, IHasDuration 
{
    float dieTime = float.MaxValue;

    public float rotSpeed = 45f;   

	void Update () 
    {
        if (Time.time >= dieTime)
        {
            Destroy(gameObject);
        }
        else
        {
            transform.Rotate(new Vector3(0, rotSpeed * Time.deltaTime, 0));
        }
	}

    public void SetDuration(float dur)
    {
        dieTime = Time.time + dur;
    }
}
