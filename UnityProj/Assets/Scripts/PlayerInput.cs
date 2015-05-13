﻿using Engine;
using System;
using UnityEngine;
using System.Collections.Generic;

class PlayerInput : MonoBehaviour
{
    readonly Dictionary<KeyCode, RuneType> runeKeys = new Dictionary<KeyCode, RuneType>();

    public PlayerInput()
    {
        runeKeys.Add(KeyCode.Slash,         RuneType.Compile);
        runeKeys.Add(KeyCode.W,             RuneType.Arrow0);
        runeKeys.Add(KeyCode.A,             RuneType.ArrowL60);
        runeKeys.Add(KeyCode.D,             RuneType.ArrowR60);
        runeKeys.Add(KeyCode.Z,             RuneType.ArrowL120);
        runeKeys.Add(KeyCode.C,             RuneType.ArrowR120);
        runeKeys.Add(KeyCode.S,             RuneType.ArrowCross);

        runeKeys.Add(KeyCode.G,             RuneType.Avatar);
        runeKeys.Add(KeyCode.T,             RuneType.AvatarWalkDir);
        runeKeys.Add(KeyCode.R,             RuneType.AvatarWalkDirDraw);
        runeKeys.Add(KeyCode.Y,             RuneType.AvatarForward);
        runeKeys.Add(KeyCode.U,             RuneType.AvatarForwardDraw);
        runeKeys.Add(KeyCode.F,             RuneType.AvatarLeft);
        runeKeys.Add(KeyCode.H,             RuneType.AvatarRight);

        runeKeys.Add(KeyCode.V,             RuneType.Flame);
        runeKeys.Add(KeyCode.B,             RuneType.Stone);
        runeKeys.Add(KeyCode.N,             RuneType.Wind);

        runeKeys.Add(KeyCode.Alpha2,        RuneType.Number2);
        runeKeys.Add(KeyCode.Alpha3,        RuneType.Number3);
        runeKeys.Add(KeyCode.Alpha4,        RuneType.Number4);
        runeKeys.Add(KeyCode.Alpha5,        RuneType.Number5);
        runeKeys.Add(KeyCode.Alpha6,        RuneType.Number6);
        runeKeys.Add(KeyCode.Alpha7,        RuneType.Number7);

        runeKeys.Add(KeyCode.I,             RuneType.If);
        runeKeys.Add(KeyCode.O,             RuneType.PredicateAvatarRef);
        runeKeys.Add(KeyCode.K,             RuneType.PredicateTileEmpty);
        runeKeys.Add(KeyCode.L,             RuneType.PredicateTileWall);
        runeKeys.Add(KeyCode.P,             RuneType.PredicateTileMonster);
    }
    
    

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
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            D.playerMove(getMouseOverTile());
        }


        foreach (var kvp in runeKeys)        
            if (Input.GetKeyDown(kvp.Key))
                D.playerDrawOrEraseRune(kvp.Value, getMouseOverTile(), false);

        if (Input.GetKeyDown(KeyCode.Space))
            D.playerDrawOrEraseRune(RuneType.Arrow0, getMouseOverTile(), true);
        
            

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

