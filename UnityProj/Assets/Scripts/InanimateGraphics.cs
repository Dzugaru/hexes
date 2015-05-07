using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Engine;

class InanimateGraphics : EntityGraphics
{
    unsafe public void DispatchOp(EntityOperation op, void* args)
    {
        switch (op)
        {
            case EntityOperation.Spawn: Spawn(*(HexXY*)args); break;
            case EntityOperation.Die: Die(); break;
        }
    }

    void Spawn(HexXY pos)
    {       
        Vector2 planeCoord = pos.ToPlaneCoordinates();
        transform.position = new Vector3(planeCoord.x, 0, planeCoord.y);
        gameObject.SetActive(true);
    }

    void Die()
    {
        Destroy(gameObject);       
    }
}

