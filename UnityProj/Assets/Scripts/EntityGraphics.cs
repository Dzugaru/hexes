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
    public int entityType;

    [HideInInspector]
    public D.EntityHandle entityHandle;

    [StructLayout(LayoutKind.Sequential)]
    protected struct MoveArgs
    {
        public HexXY pos;
        public float timeToGetThere;
    }

    unsafe public void DispatchOp(EntityOperation op, void* args)
    {
        //Debug.Log(op);
        switch (op)
        {
            case EntityOperation.Spawn: Spawn(*(HexXY*)args); break;
            case EntityOperation.Move: Move(*(MoveArgs*)args); break;
            case EntityOperation.Stop: Stop(*(HexXY*)args); break;
            case EntityOperation.Attack: Attack(*(HexXY*)args); break;
            case EntityOperation.Damage: Damage(*(float*)args); break;
            case EntityOperation.UpdateInfo: UpdateInfo(args); break;
            case EntityOperation.Die: Die(); break;
        }
    }

    protected virtual void Spawn(HexXY p) { }
    protected virtual void Move(MoveArgs args) { }
    protected virtual void Stop(HexXY p) { }
    protected virtual void Attack(HexXY p) { }
    protected virtual void Damage(float dmg) { }
    unsafe protected virtual void UpdateInfo(void* args) { }
    protected virtual void Die() { }
}

