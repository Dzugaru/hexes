using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Highlight
{
    EntityGraphics mainGraphics;

    GameObject highlightMeshCopy;
    Material highlightMat;
    Coroutine flashHighlightAnimation; //These don't support state view (exec, term), wtf
    bool isflashHighlightAnimationRunning;

    public bool isHighlightEnabled;

    public Highlight(EntityGraphics mainGraphics)
    {
        this.mainGraphics = mainGraphics;
    }

    public void EnableHighlight()
    {
        isHighlightEnabled = true;
        Material hlMat = (Material)Resources.Load("Debug/Highlight");
        highlightMat = new Material(hlMat);

        highlightMeshCopy = GameObject.Instantiate(mainGraphics.meshRoot);
        highlightMeshCopy.transform.SetParent(mainGraphics.meshRoot.transform.parent, false);

        foreach (var mr in highlightMeshCopy.GetComponentsInChildren<MeshRenderer>())
            mr.sharedMaterial = highlightMat;

        foreach (var mr in highlightMeshCopy.GetComponentsInChildren<SkinnedMeshRenderer>())
            mr.sharedMaterial = highlightMat;       
    }

    public void DisableHighlight()
    {
        isHighlightEnabled = false;
        GameObject.Destroy(highlightMat);
        GameObject.Destroy(highlightMeshCopy);
    }

    public void AnimateFlashHighlight(Color color, float dur, float peakAlpha)
    {        
        bool isHighlightAlreadyPresent;
        if (isflashHighlightAnimationRunning)
        {
            mainGraphics.StopCoroutine(flashHighlightAnimation);
            isHighlightAlreadyPresent = true;
        }
        else
        {
            isHighlightAlreadyPresent = false;
        }
        flashHighlightAnimation = mainGraphics.StartCoroutine(CorAnimateFlashHighlight(color, dur, peakAlpha, isHighlightAlreadyPresent));
    }

    IEnumerator CorAnimateFlashHighlight(Color color, float dur, float peakAlpha, bool isHighlightAlreadyPresent)
    {
        isflashHighlightAnimationRunning = true;
        if (!isHighlightAlreadyPresent) EnableHighlight();
        highlightMat.color = new Color(color.r, color.g, color.b, 0);
        float startTime = Time.time;

        for (; ;)
        {
            if (Time.time > startTime + dur) break;
            Color col = highlightMat.color;
            highlightMat.color = new Color(col.r, col.g, col.b, Mathf.Sin((Time.time - startTime) / dur * Mathf.PI) * peakAlpha);

            yield return null;
        }

        DisableHighlight();
        isflashHighlightAnimationRunning = false;
        yield break;
    }
}

