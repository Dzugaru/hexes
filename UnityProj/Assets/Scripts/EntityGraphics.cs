﻿using System;
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

    [HideInInspector]
    public Interfacing.EntityHandle entityHandle;  

    public virtual void Spawn(HexXY p) { }
    public virtual void Move(HexXY pos, float timeToGetThere) { }
    public virtual void Stop(HexXY p) { }
    public virtual void Attack(HexXY p) { }
    public virtual void Damage(float dmg) { }
    public virtual void Die() { }
    public virtual void UpdateHP(float currentHP, float maxHP) { }
}

