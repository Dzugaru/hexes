using Engine;
using System;
using UnityEngine;

class PlayerInput : MonoBehaviour
{
    HexXY getMouseOverTile()
    {
        float dist;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (G.gamePlane.Raycast(ray, out dist))
        {
            Vector3 inter = ray.origin + dist * ray.direction;
            Vector2 planePos = new Vector2(inter.x, inter.z);
            return HexXY.FromPlaneCoordinates(planePos);
        }
        else
        {
            return new HexXY(0, 0);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            D.playerMove(getMouseOverTile());
        }

        if (Input.GetMouseButtonDown(0))
        {
            D.playerCastSpell(SpellType.LineOfFire, getMouseOverTile());
        }

        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    G.g.isTimeStopped = !G.g.isTimeStopped;
        //    if (G.g.isTimeStopped)
        //    {
        //        Time.timeScale = 0.1f;
        //    }
        //    else
        //    {
        //        Time.timeScale = 1;
        //    }
        //}
    }
}

