using UnityEngine;
using System.Collections;

public class LevelEditorCameraControl : MonoBehaviour
{
    static readonly Plane gamePlane = new Plane(new Vector3(0, 1, 0), 0);

    new Camera camera;

    public float scrollSpeed, scrollMargin, rotateSpeed, zoomSpeed, minZoomDist, maxZoomDist;
    public bool boundToPlayer;
    public float playerCatchupSpeed;

    public GameObject player;

    Vector2? oldMousePos;

    void Start()
    {
        camera = GetComponent<Camera>();
    }
	
	void Update ()
    {
        Ray viewRay = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        float dist;
        gamePlane.Raycast(viewRay, out dist);

        //Zoom
        float wheel = Input.GetAxis("Mouse ScrollWheel");
        if (wheel != 0)
        {             
            float newDist = Mathf.Clamp(dist - wheel * zoomSpeed, minZoomDist, maxZoomDist);            
            float diff = newDist - dist;          
            if (diff != 0)
            {
                transform.position -= viewRay.direction * diff;
            }            
        }

       
        Vector2 mousePos = Input.mousePosition;
        if (mousePos.x >= 0 && mousePos.x < Screen.width && mousePos.y >= 0 && mousePos.y < Screen.height)
        {
            //Rotate        
            if (Input.GetMouseButton(1))
            {
                if (oldMousePos.HasValue)
                {
                    var deltaX = mousePos.x - oldMousePos.Value.x;
                    transform.RotateAround(transform.position + viewRay.direction * dist, new Vector3(0, 1, 0), Time.deltaTime * rotateSpeed * deltaX);
                }
            }
            //Scroll 
            else
            {
                //if (mousePos.x < scrollMargin)
                if(Input.GetKey(KeyCode.A))
                    transform.position -= transform.right * scrollSpeed * Time.deltaTime;

                //if (mousePos.x > Screen.width - scrollMargin)
                if (Input.GetKey(KeyCode.D))
                    transform.position += transform.right * scrollSpeed * Time.deltaTime;

                Vector3 fwd = transform.forward;
                fwd.y = 0;

                //if (mousePos.y < scrollMargin)
                if (Input.GetKey(KeyCode.S))
                    transform.position -= fwd * scrollSpeed * Time.deltaTime;

                //if (mousePos.y > Screen.height - scrollMargin)
                if (Input.GetKey(KeyCode.W))
                    transform.position += fwd * scrollSpeed * Time.deltaTime;
            }
        }

        oldMousePos = mousePos;

    }
}
