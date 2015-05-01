using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

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

    State state;
    Vector2 walkDest;
    Vector3 walkDir;
    float walkSpeed;

    Quaternion oldRotation, targetRotation;
    float rotationFixLeft;
    

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

