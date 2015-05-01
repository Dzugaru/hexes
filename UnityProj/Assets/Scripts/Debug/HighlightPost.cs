using System;
using UnityEngine;

class HighlightPost : MonoBehaviour
{
    HighlightCam hlCam;

    void Awake()
    {
        hlCam = transform.GetComponentInChildren<HighlightCam>();
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {        
        Graphics.Blit(source, destination, hlCam.highlightBlender);
    }
}

