using Engine;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

class CharacterGraphics : EntityGraphics, IHighlightable
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
    
    public float hpBarUpDistance;

    Animator mecanimAnimator;
    Animation legacyAnimator;

    State state;
    Vector2 walkDest;
    Vector3 walkDir;
    float walkSpeed;

    Quaternion oldRotation, targetRotation;
    float rotationFixLeft;

    Coroutine attackCor;

   
    public float currentHP;
    public float maxHP;
   

    public static List<CharacterGraphics> activeCharacters = new List<CharacterGraphics>();


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

    public override void Spawn(HexXY pos)
    {
        //if(entityHandle.idx == 7) Debug.Log(entityHandle + " spawn " + pos);
        Vector2 planeCoord = pos.ToPlaneCoordinates();
        transform.position = new Vector3(planeCoord.x, 0, planeCoord.y);
        gameObject.SetActive(true);        

        if (entity.entityClass == EntityClass.Character && entityType == (int)CharacterType.Player)
        {
            Camera.main.GetComponent<CameraControl>().player = gameObject;
        }

        activeCharacters.Add(this);
    }

    public override void Move(HexXY pos, float timeToGetThere)
    {
        //Debug.Log(entityHandle + " move " + args.pos);      
        state = State.Walk;
        Vector2 dest = pos.ToPlaneCoordinates();
        walkDest = dest;
        float dist = (new Vector2(transform.position.x, transform.position.z) - dest).magnitude;
        walkSpeed = dist / timeToGetThere;
        walkDir = (new Vector3(walkDest.x, 0, walkDest.y) - transform.position).normalized;
        RotateTo(walkDir);

        if (animationsType == AnimationsType.Legacy) legacyAnimator.Play("Walk");
        else if(animationsType == AnimationsType.Mecanim) mecanimAnimator.SetBool("IsWalking", true);
    }

    public override void Stop(HexXY pos)
    {
        //Debug.Log(entityHandle + " stop " + walkDest);
        state = State.Stand;
        Vector2 planePos = pos.ToPlaneCoordinates();
        transform.position = new Vector3(planePos.x, 0, planePos.y);

        if (animationsType == AnimationsType.Legacy) legacyAnimator.Play("Idle");
        else if (animationsType == AnimationsType.Mecanim) mecanimAnimator.SetBool("IsWalking", false);
    }

    public override void Attack(HexXY pos)
    {        
        if (attackCor != null)        
            StopCoroutine(attackCor);

        attackCor = StartCoroutine(CorAttack(pos));
    }

    public override void Damage(float dmg)
    {        
        highlight.AnimateFlashHighlight(Color.white, 0.15f, 0.5f);
    }

    public override void UpdateHP(float currentHP, float maxHP)
    {
        this.currentHP = currentHP;
        this.maxHP = maxHP;
    }

    public override void Die()
    {
        Destroy(gameObject);
        activeCharacters.Remove(this);
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

   


    void Update()
    {
        UpdateObjectMovement();
    }    
}

