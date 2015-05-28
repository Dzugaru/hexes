using Engine;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class PlayerInput : MonoBehaviour
{
    public static readonly Dictionary<KeyCode, RuneType> runeKeys = new Dictionary<KeyCode, RuneType>();

    static PlayerInput()
    {
        runeKeys.Add(KeyCode.X, RuneType.Nop);
        runeKeys.Add(KeyCode.W, RuneType.Arrow0);
        runeKeys.Add(KeyCode.A, RuneType.ArrowL60);
        runeKeys.Add(KeyCode.D, RuneType.ArrowR60);
        runeKeys.Add(KeyCode.Z, RuneType.ArrowL120);
        runeKeys.Add(KeyCode.C, RuneType.ArrowR120);
        runeKeys.Add(KeyCode.S, RuneType.ArrowCross);

        runeKeys.Add(KeyCode.T, RuneType.AvatarWalkDir);
        runeKeys.Add(KeyCode.R, RuneType.AvatarWalkDirDraw);
        runeKeys.Add(KeyCode.Y, RuneType.AvatarForward);
        runeKeys.Add(KeyCode.U, RuneType.AvatarForwardDraw);
        runeKeys.Add(KeyCode.F, RuneType.AvatarLeft);
        runeKeys.Add(KeyCode.H, RuneType.AvatarRight);

        runeKeys.Add(KeyCode.V, RuneType.Flame);
        runeKeys.Add(KeyCode.B, RuneType.Stone);
        runeKeys.Add(KeyCode.N, RuneType.Wind);

        runeKeys.Add(KeyCode.Alpha2, RuneType.Number2);
        runeKeys.Add(KeyCode.Alpha3, RuneType.Number3);
        runeKeys.Add(KeyCode.Alpha4, RuneType.Number4);
        runeKeys.Add(KeyCode.Alpha5, RuneType.Number5);
        runeKeys.Add(KeyCode.Alpha6, RuneType.Number6);
        runeKeys.Add(KeyCode.Alpha7, RuneType.Number7);

        runeKeys.Add(KeyCode.I, RuneType.If);
        runeKeys.Add(KeyCode.O, RuneType.PredicateAvatarRef);
        runeKeys.Add(KeyCode.K, RuneType.PredicateTileEmpty);
        runeKeys.Add(KeyCode.L, RuneType.PredicateTileWall);
        runeKeys.Add(KeyCode.P, RuneType.PredicateTileMonster);
    }

    EntityGraphics mouseOverClickable;
    

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
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

        foreach (var kvp in runeKeys)        
            if (Input.GetKeyDown(kvp.Key))
                E.player.DrawRune(kvp.Value, getMouseOverTile());

        if (Input.GetKeyDown(KeyCode.Space))
            E.player.EraseRune(getMouseOverTile());

        //if (Input.GetKeyDown(KeyCode.Return))
        //{
        //    if (E.PlayerCompileSpell(E.player.pos))
        //    {
        //        //DEBUG save it
        //        using (var writer = new BinaryWriter(File.OpenWrite(Path.Combine(Application.persistentDataPath, "spell"))))
        //            E.player.currentSpell.Save(writer);
        //    }
        //    else
        //    {
        //        Logger.Log("Can't compile this");
        //    }
        //}

        //if (Input.GetKeyDown(KeyCode.Backspace))
        //    E.PlayerCastSpell(getMouseOverTile());

        //DEBUG redraw spell
        //if (Input.GetKeyDown(KeyCode.Backslash) && E.player.currentSpell != null)            
        //    E.player.currentSpell.RedrawOnGround(E.player.currentSpell.root, E.player.pos);

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Camera.main.GetComponent<CameraControl>().IsInRuneDrawingMode = !Camera.main.GetComponent<CameraControl>().IsInRuneDrawingMode;
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

        if (Input.GetKeyDown(KeyCode.F1))
            E.player.selectedAbilitySpell = 0;             
        if (Input.GetKeyDown(KeyCode.F2))
            E.player.selectedAbilitySpell = 1;
        if (Input.GetKeyDown(KeyCode.F3))
            E.player.selectedAbilitySpell = 2;

        bool isClickedObject = MouseOverHighlightAndClick();
        if (!isClickedObject && Input.GetMouseButtonDown(0))
        {
            if (E.player.selectedAbilitySpell != -1 && E.player.abilitySpells[E.player.selectedAbilitySpell] != null)
            {
                E.player.CastAbilitySpell(E.player.selectedAbilitySpell, getMouseOverTile());
                E.player.selectedAbilitySpell = -1;
            }
            else
            {
                E.player.Move(getMouseOverTile());
            }          
        }
    }

    bool MouseOverHighlightAndClick()
    {        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool isFound = false;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.point.y >= 0)
            {
                var go = hit.transform.gameObject;
                //Debug.Log("hit " + go);
                var gr = go.GetComponentInParent<EntityGraphics>();

                if (gr != null && gr is IClickable)
                {
                    isFound = true;
                    if (!((IHighlightable)gr).CanBeHighlighted)
                    {
                        if (gr.highlight.isHighlightEnabled)
                        {
                            gr.highlight.DisableHighlight();
                            mouseOverClickable = null;
                        }
                    }
                    else
                    {
                        if (gr != mouseOverClickable)
                        {
                            if (mouseOverClickable != null)
                                mouseOverClickable.highlight.DisableHighlight();

                            mouseOverClickable = gr;
                            gr.highlight.EnableHighlight();
                        }
                    }
                }
            }
        }

        if (!isFound && mouseOverClickable != null)
        {
            mouseOverClickable.highlight.DisableHighlight();
            mouseOverClickable = null;
        }

        if (mouseOverClickable != null && Input.GetMouseButtonDown(0))
        {            
            E.player.Clicked(mouseOverClickable.entity);
            return true;
        }

        return false;
    }    
}

