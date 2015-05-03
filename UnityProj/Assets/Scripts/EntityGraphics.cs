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
        Walk,
        Attack
    }

    public AnimationsType animationsType;
    public GameObject meshRoot;

    Animator mecanimAnimator;
    Animation legacyAnimator;

    State state;
    Vector2 walkDest;
    Vector3 walkDir;
    float walkSpeed;

    Quaternion oldRotation, targetRotation;
    float rotationFixLeft;

    Coroutine attackCor;

    GameObject highlightMeshCopy;
    Material highlightMat;
    Coroutine flashHighlightAnimation; //These don't support state view (exec, term), wtf
    bool isflashHighlightAnimationRunning;

    public GrObjType grObjType;
    public D.GrObjHandle grObjHandle;


    void Awake()
    {
        state = State.Stand;
        if (animationsType == AnimationsType.Legacy)
        {
            var legacySettings = GetComponent<LegacyAnimSettings>();

            if (legacySettings != null)
            {
                legacyAnimator = transform.GetChild(0).GetComponent<Animation>();
                legacyAnimator["Walk"].speed = legacySettings.walkSpeed;
                legacyAnimator["Attack"].speed = legacySettings.attackSpeed;
            }
        }
        else if (animationsType == AnimationsType.Mecanim)
        {
             mecanimAnimator = transform.GetChild(0).GetComponent<Animator>();
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
            case GrObjOperation.Stop: Stop(*(HexXY*)args); break;
            case GrObjOperation.Attack: Attack(*(HexXY*)args); break;
        }
    }  

    void Spawn(HexXY pos)
    {
        //if(grObjHandle.idx == 7) Debug.Log(grObjHandle + " spawn " + pos);
        Vector2 planeCoord = pos.ToPlaneCoordinates();
        transform.position = new Vector3(planeCoord.x, 0, planeCoord.y);
        gameObject.SetActive(true);
        //if (grObjHandle.idx == 7) Debug.Log(grObjHandle + " " + transform.position);

        if (grObjType == GrObjType.Player)
        {
            Camera.main.GetComponent<CameraControl>().player = gameObject;
        }
    }

    void Move(MoveArgs args)
    {
        //if (grObjHandle.idx == 7) Debug.Log(grObjHandle + " move " + args.pos);
        //Debug.Log("Move: " + args.pos + " " + args.timeToGetThere);
        state = State.Walk;
        Vector2 dest = args.pos.ToPlaneCoordinates();
        walkDest = dest;
        float dist = (new Vector2(transform.position.x, transform.position.z) - dest).magnitude;
        walkSpeed = dist / args.timeToGetThere;
        walkDir = (new Vector3(walkDest.x, 0, walkDest.y) - transform.position).normalized;
        RotateTo(walkDir);

        if (animationsType == AnimationsType.Legacy) legacyAnimator.Play("Walk");
        else if(animationsType == AnimationsType.Mecanim) mecanimAnimator.SetBool("IsWalking", true);
    }

    void Stop(HexXY pos)
    {
        //Debug.Log(grObjHandle + " stop " + walkDest);
        state = State.Stand;
        Vector2 planePos = pos.ToPlaneCoordinates();
        transform.position = new Vector3(planePos.x, 0, planePos.y);

        if (animationsType == AnimationsType.Legacy) legacyAnimator.Play("Idle");
        else if (animationsType == AnimationsType.Mecanim) mecanimAnimator.SetBool("IsWalking", false);
    }

    void Attack(HexXY pos)
    {
        if (attackCor != null)        
            StopCoroutine(attackCor);

        attackCor = StartCoroutine(CorAttack(pos));
    }

    IEnumerator CorAttack(HexXY pos)
    {
        //Debug.Log("Attack " + pos);
        if (animationsType == AnimationsType.Legacy)
        {
            if (state == State.Attack) legacyAnimator.Rewind("Attack");
            legacyAnimator.Play("Attack");
        }
        //TODO: mecanim attack

        state = State.Attack;
        Vector2 planePos = pos.ToPlaneCoordinates();
        Vector3 dir = new Vector3(planePos.x, 0, planePos.y) - transform.position;
        RotateTo(dir);

        legacyAnimator.PlayQueued("Idle");

        //yield return new WaitForSeconds(2.0f);
        //if (state == State.Attack)
        //{
        //    if (animationsType == AnimationsType.Legacy) legacyAnimator.Play("Idle");
        //    state = State.Stand;
        //}

        yield break;      
    }

    void RotateTo(Vector3 dir)
    {
        targetRotation = Quaternion.LookRotation(dir);
        if (transform.rotation != targetRotation)
        {
            rotationFixLeft = 1;
            oldRotation = transform.rotation;
        }
    }

    void UpdateObjectMovement()
    {
        //Update rotation
        if (rotationFixLeft > 0)
        {
            rotationFixLeft = Mathf.Max(0, rotationFixLeft - Time.deltaTime * 5);
            if (rotationFixLeft == 0)
                transform.rotation = targetRotation;
            else
                transform.rotation = Quaternion.Lerp(targetRotation, oldRotation, rotationFixLeft);
        }

        //Update walking
        if (state == State.Walk)
        {
            Vector3 diff = new Vector3(walkDest.x, 0, walkDest.y) - transform.position;
            if (walkSpeed * Time.deltaTime >= diff.magnitude)            
                transform.position = new Vector3(walkDest.x, 0, walkDest.y);            
            else            
                transform.position += walkSpeed * Time.deltaTime * walkDir;                           
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
        UpdateObjectMovement();
    }    
}

