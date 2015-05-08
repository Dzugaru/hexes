using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Engine;

public class InanimateGraphics : EntityGraphics
{
    protected override void Spawn(HexXY pos)
    {       
        Vector2 planeCoord = pos.ToPlaneCoordinates();
        transform.position = new Vector3(planeCoord.x, 0, planeCoord.y);
        gameObject.SetActive(true);
    }

    protected override void Die()
    {
        Destroy(gameObject);       
    }
}

