using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Engine;
using System.Runtime.InteropServices;

public abstract class EntityGraphics : MonoBehaviour
{
    [HideInInspector]
    public uint entityType;

    public Entity entity;

    public virtual void Spawn(HexXY p) { }
    public virtual void Move(HexXY pos, float timeToGetThere) { }
    public virtual void Stop(HexXY p) { }
    public virtual void Attack(HexXY p) { }
    public virtual void Damage(float dmg) { }
    public virtual void Die() { }
    public virtual void UpdateHP(float currentHP, float maxHP) { }

    public Highlight highlight;
    public GameObject meshRoot;

    void Awake()
    {
        if (this is IHighlightable)        
            highlight = new Highlight(this);
        
    }

    public void UpdateInterfaceRotation(uint dir)
    {
        transform.rotation = Quaternion.AngleAxis(dir * 60, new Vector3(0, 1, 0));        
    }
}

