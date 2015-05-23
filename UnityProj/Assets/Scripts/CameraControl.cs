using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour
{
    new Camera camera;

    public float scrollSpeed, scrollMargin, zoomSpeed, minZoomDist, maxZoomDist;
    public bool boundToPlayer;
    public float playerCatchupSpeed;
    public float dipAngle;
    public float rotateSpeed;

    public GameObject player;


    bool isInRuneDrawingMode;
    public bool IsInRuneDrawingMode
    {
        get
        {
            return isInRuneDrawingMode;
        }
        set
        {
            if (isInRuneDrawingMode != value)
            {
                isInRuneDrawingMode = value;
                runeDrawingChangeProgress = 1;
            }
        }
    }

    float runeDrawingChangeProgress;
    Vector2? oldMousePos;

    void Awake()
    {
        camera = GetComponent<Camera>();
        dipAngle = camera.transform.rotation.eulerAngles.x;
    }

    public void SetOnPlayerInstantly()
    {
        Ray viewRay = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        float dist;
        G.gamePlane.Raycast(viewRay, out dist);

        Vector2 playerPlanePos = new Vector2(player.transform.position.x, player.transform.position.z);
        Vector3 cameraPlaneLookPos3D = transform.position + viewRay.direction * dist;
        Vector2 cameraPlaneLookPos = new Vector2(cameraPlaneLookPos3D.x, cameraPlaneLookPos3D.z);
        float catchupDist = (playerPlanePos - cameraPlaneLookPos).magnitude;      
        Vector2 catchupDir2D = (playerPlanePos - cameraPlaneLookPos).normalized;
        Vector3 catchupDir3D = new Vector3(catchupDir2D.x, 0, catchupDir2D.y);
        camera.transform.position += catchupDist * catchupDir3D;
    }
	
	void Update ()
    {
        Ray viewRay = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        float dist;
        G.gamePlane.Raycast(viewRay, out dist);

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

        //Scroll
        if (!boundToPlayer)
        {
            //if (Input.mousePosition.x < scrollMargin)
            //    transform.position -= transform.right * scrollSpeed;

            //if (Input.mousePosition.x > Screen.width - scrollMargin)
            //    transform.position += transform.right * scrollSpeed;

            //Vector3 fwd = transform.forward;
            //fwd.y = 0;

            //if (Input.mousePosition.y < scrollMargin)
            //    transform.position -= fwd * scrollSpeed;

            //if (Input.mousePosition.y > Screen.height - scrollMargin)
            //    transform.position += fwd * scrollSpeed;
        }
        else
        {
            if (player != null)
            {
                Vector2 playerPlanePos = new Vector2(player.transform.position.x, player.transform.position.z);
                Vector3 cameraPlaneLookPos3D = transform.position + viewRay.direction * dist;
                Vector2 cameraPlaneLookPos = new Vector2(cameraPlaneLookPos3D.x, cameraPlaneLookPos3D.z);
                float catchupDist = (playerPlanePos - cameraPlaneLookPos).magnitude;
                float catchupStep = Mathf.Min(Mathf.Sqrt(catchupDist) * playerCatchupSpeed, catchupDist);
                Vector2 catchupDir2D = (playerPlanePos - cameraPlaneLookPos).normalized;
                Vector3 catchupDir3D = new Vector3(catchupDir2D.x, 0, catchupDir2D.y);
                camera.transform.position += catchupStep * catchupDir3D;
            }
        }



        //Rune draw rotate (Shift)
        if (runeDrawingChangeProgress != 0)
        {
            float delta;
            if (IsInRuneDrawingMode)
            {
                float oldAngle = Mathf.SmoothStep(90, dipAngle, runeDrawingChangeProgress);
                runeDrawingChangeProgress -= Time.deltaTime;
                delta = Mathf.SmoothStep(90, dipAngle, runeDrawingChangeProgress) - oldAngle;
            }
            else
            {
                float oldAngle = Mathf.SmoothStep(dipAngle, 90, runeDrawingChangeProgress);
                runeDrawingChangeProgress -= Time.deltaTime;
                delta = Mathf.SmoothStep(dipAngle, 90, runeDrawingChangeProgress) - oldAngle;
            }

            camera.transform.RotateAround(transform.position + viewRay.direction * dist, camera.transform.right, delta);
        }
        else
        {
            //Camera rotate
            if (Input.GetMouseButton(0) && Input.GetMouseButton(1))
            {
                var mousePos = Input.mousePosition;

                if (oldMousePos.HasValue)
                {
                    var deltaX = mousePos.x - oldMousePos.Value.x;
                    transform.RotateAround(transform.position + viewRay.direction * dist, new Vector3(0, 1, 0), Time.deltaTime * rotateSpeed * deltaX);
                }

                oldMousePos = mousePos;
            }
            else
            {
                oldMousePos = null;
            }
        }
    }
}
