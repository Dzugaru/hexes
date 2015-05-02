using Engine;
using System;
using UnityEngine;

class PlayerInput : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            float dist;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (G.gamePlane.Raycast(ray, out dist))
            {
                Vector3 inter = ray.origin + dist * ray.direction;
                Vector2 planePos = new Vector2(inter.x, inter.z);
                HexXY hexPos = HexXY.FromPlaneCoordinates(planePos);
                D.playerMove(hexPos);
            }
        }
    }
}

