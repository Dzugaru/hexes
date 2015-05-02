using Engine;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections;

class EntityGraphics : MonoBehaviour
{
    public enum AnimationsType
    {
        Mecanim,
        Legacy,
        Nothing
    }

    public enum State
    {
        Stand,
        Walk
    }

    public AnimationsType animationsType;
    public GameObject meshRoot;

    State state;
    Vector2 walkDest;
    Vector3 walkDir;
    float walkSpeed;

    Quaternion oldRotation, targetRotation;
    float rotationFixLeft;

    GameObject highlightMeshCopy;
    Material highlightMat;
    Coroutine flashHighlightAnimation; //These don't support state view (exec, term), wtf
    bool isflashHighlightAnimationRunning;

    public GrObjType grObjType;


    void Awake()
    {
        state = State.Stand;
        if (animationsType == AnimationsType.Legacy)
        {
            var legacySettings = GetComponent<LegacyAnimSettings>();
            
            if (legacySettings != null)
            {
                var anim = transform.GetChild(0).GetComponent<Animation>();
                
                anim["Walk"].speed = legacySettings.walkSpeed;
            }
        }     
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MoveArgs
    {
        public HexXY pos;
        public float timeToGetThere;
    }

    unsafe public void DispatchOp(GrObjOperation op, void* args)
    {
        switch (op)
        {
            case GrObjOperation.Spawn: Spawn(*(HexXY*)args); break;
            case GrObjOperation.Move: Move(*(MoveArgs*)args); break;
            case GrObjOperation.Stop: Stop(); break;
        }
    }  

    void Spawn(HexXY pos)
    {
        Vector2 planeCoord = pos.ToPlaneCoordinates();
        transform.position = new Vector3(planeCoord.x, 0, planeCoord.y);
        gameObject.SetActive(true);

        if (grObjType == GrObjType.Player)
        {
            Camera.main.GetComponent<CameraControl>().player = gameObject;
        }
    }

    void Move(MoveArgs args)
    {
        //Debug.Log("Move: " + args.pos + " " + args.timeToGetThere);
        state = State.Walk;
        Vector2 dest = args.pos.ToPlaneCoordinates();
        walkDest = dest;
        float dist = (new Vector2(transform.position.x, transform.position.z) - dest).magnitude;
        walkSpeed = dist / args.timeToGetThere;
        walkDir = (new Vector3(walkDest.x, 0, walkDest.y) - transform.position).normalized;
        targetRotation = Quaternion.LookRotation(walkDir);
        if (transform.rotation != targetRotation)
        {
            rotationFixLeft = 1;
            oldRotation = transform.rotation;
        }
    }

    void Stop()
    {
        state = State.Stand;
        transform.position = new Vector3(walkDest.x, 0, walkDest.y);
    }

    void UpdateMecanimAnimations()
    {
        var animator = transform.GetChild(0).GetComponent<Animator>();
        animator.SetBool("IsWalking", state == State.Walk);        
    }

    void UpdateLegacyAnimations()
    {
        var anim = transform.GetChild(0).GetComponent<Animation>();

        if (state == State.Stand && !anim.IsPlaying("Idle"))
        {            
            anim.Play("Idle", PlayMode.StopAll);
        }
        else if (state == State.Walk && !anim.IsPlaying("Walk"))
        {            
            anim.Play("Walk", PlayMode.StopAll);
        }
    }

    void UpdateObjectMovement()
    {
        if (state == State.Walk)
        {
            Vector3 diff = new Vector3(walkDest.x, 0, walkDest.y) - transform.position;
            if (walkSpeed * Time.deltaTime >= diff.magnitude)
            {
                transform.position = new Vector3(walkDest.x, 0, walkDest.y);
            }
            else
            {
                transform.position += walkSpeed * Time.deltaTime * walkDir;
                if (rotationFixLeft > 0)
                {
                    rotationFixLeft = Mathf.Max(0, rotationFixLeft - Time.deltaTime * 5);
                    if (rotationFixLeft == 0)
                        transform.rotation = targetRotation;
                    else
                        transform.rotation = Quaternion.Lerp(targetRotation, oldRotation, rotationFixLeft);
                }
            }
        }
    }

    void EnableHighlight()
    {
        Material hlMat = (Material)Resources.Load("Debug/Highlight");
        highlightMat = new Material(hlMat);        

        highlightMeshCopy = Instantiate(meshRoot);
        highlightMeshCopy.transform.parent = meshRoot.transform.parent;

        foreach (var mr in highlightMeshCopy.GetComponentsInChildren<MeshRenderer>())        
            mr.sharedMaterial = highlightMat;                    

        foreach (var mr in highlightMeshCopy.GetComponentsInChildren<SkinnedMeshRenderer>())
            mr.sharedMaterial = highlightMat;        
    }

    void DisableHighlight()
    {
        Destroy(highlightMeshCopy);        
    }

    public void AnimateFlashHighlight(Color color, float dur, float peakAlpha)
    {
        bool isHighlightAlreadyPresent;
        if (isflashHighlightAnimationRunning)
        {
            StopCoroutine(flashHighlightAnimation);
            isHighlightAlreadyPresent = true;
        }
        else
        {
            isHighlightAlreadyPresent = false;
        }        
        flashHighlightAnimation = StartCoroutine(CorAnimateFlashHighlight(color, dur, peakAlpha, isHighlightAlreadyPresent));
    }

    IEnumerator CorAnimateFlashHighlight(Color color, float dur, float peakAlpha, bool isHighlightAlreadyPresent)
    {
        isflashHighlightAnimationRunning = true;
        if(!isHighlightAlreadyPresent) EnableHighlight();
        highlightMat.color = new Color(color.r, color.g, color.b, 0);
        float startTime = Time.time;

        for(; ;)
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


    void Update()
    {
        switch (animationsType)
        {
            case AnimationsType.Mecanim: UpdateMecanimAnimations(); break;
            case AnimationsType.Legacy: UpdateLegacyAnimations(); break;
        }

        UpdateObjectMovement();
    }    
}

