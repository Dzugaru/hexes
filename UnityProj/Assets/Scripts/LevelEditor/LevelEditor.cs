using UnityEngine;
using System.Collections;
using Engine;
using System.Diagnostics;
using UnityEngine.UI;
using System.Collections.Generic;

public class LevelEditor : MonoBehaviour
{
    public static LevelEditor S { get; private set; }
    
    Canvas canvas;

    public TerrainCellType brushCellType;
    public int brushSize;
    public bool shouldPaintOnEmpty;

    void Awake()
    {
        S = this;     

        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();        
    } 

    HexXY? brushOldMousePos;

    void Update ()
    {
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButton(0))
        {
            HexXY p = getMouseOverTile();

            if (p != brushOldMousePos)
            {
                var changedBlocks = new HashSet<WorldBlock>();

                for (int x = -(brushSize - 1); x <= brushSize - 1; x++)
                {
                    for (int y = -(brushSize - 1); y <= brushSize - 1; y++)
                    {
                        HexXY d = new HexXY(x, y);
                        if (HexXY.Dist(d) > brushSize - 1) continue;
                        HexXY pd = p + d;

                        var cellType = E.level.GetCell(pd);

                        if (cellType != brushCellType &&
                            (shouldPaintOnEmpty || cellType != TerrainCellType.Empty || brushCellType == TerrainCellType.Empty))
                        {
                            var changedWb = E.level.SetCell(pd, brushCellType);
                            if(!changedBlocks.Contains(changedWb))
                                changedBlocks.Add(changedWb);                       
                        }
                    }
                }

                foreach (var wb in changedBlocks)
                    TerrainController.S.RecreateHexTerrain(wb);
            }

            brushOldMousePos = p;            
        }
        else
        {
            brushOldMousePos = null;
        }
    }

    public void OnBrushSizeChanged(float size)
    {
        brushSize = (int)size;
    }

    public void OnPaintOnEmptyChange(bool val)
    {
        shouldPaintOnEmpty = val;
    }

    public void OnShowWalls(bool val)
    {
        TerrainController.S.IsWallsEnabled = val;
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
    
}
