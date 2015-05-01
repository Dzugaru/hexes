using System;
using UnityEngine;
using System.IO;

class HighlightCam : MonoBehaviour
{
    public float blendWeight = 0.25f;
    public Material highlightBlender;
    RenderTexture renderTex;   

    void Awake()
    {
        renderTex = new RenderTexture(Screen.width, Screen.height, 0);        
        GetComponent<Camera>().targetTexture = renderTex;
        highlightBlender.SetTexture("_Highlights", renderTex);
        
    }

    void Update()
    {
        highlightBlender.SetFloat("_BlendWeight", Mathf.Abs(Mathf.Sin(Time.time)) * blendWeight);

        //TODO: render each hl object separately
        var cam = GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.Color;
        cam.Render();
        transform.position = new Vector3(transform.position.x + 1, transform.position.y, transform.position.z);
        cam.clearFlags = CameraClearFlags.Nothing;
        cam.Render();
        transform.position = new Vector3(transform.position.x - 1, transform.position.y, transform.position.z);
    }
}

