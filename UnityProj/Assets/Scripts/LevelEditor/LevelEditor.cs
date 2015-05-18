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
    

    public TerrainCellType? brushCellType;
    public int brushSize;
    public bool shouldPaintOnEmpty;
    public GameObject sunLight;

    void Awake()
    {
        S = this;     

        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();        
    } 

    HexXY? brushOldMousePos;

    void Update ()
    {
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButton(0) && brushCellType.HasValue)
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

                        var cellType = E.level.GetCellType(pd);

                        if (cellType != brushCellType.Value &&
                            (shouldPaintOnEmpty || cellType != TerrainCellType.Empty || brushCellType.Value == TerrainCellType.Empty))
                        {
                            var changedWb = E.level.SetCellType(pd, brushCellType.Value);
                            if(!changedBlocks.Contains(changedWb))
                                changedBlocks.Add(changedWb);

                            HexXY locp = Level.GetLocalCoords(pd);

                            //Other blocks possible walls fix
                            if (locp.x == 0)
                            {
                                var nbl = E.level.GetBlock(changedWb.position - new HexXY(1, 0));
                                if(nbl != null && !changedBlocks.Contains(nbl))
                                    changedBlocks.Add(nbl);
                            }

                            if (locp.y == 0)
                            {
                                var nbl = E.level.GetBlock(changedWb.position - new HexXY(0, 1));
                                if (nbl != null && !changedBlocks.Contains(nbl))
                                    changedBlocks.Add(nbl);
                            }

                            if (locp.x == WorldBlock.sz - 1)
                            {
                                var nbl = E.level.GetBlock(changedWb.position + new HexXY(1, 0));
                                if (nbl != null && !changedBlocks.Contains(nbl))
                                    changedBlocks.Add(nbl);
                            }

                            if (locp.y == WorldBlock.sz - 1)
                            {
                                var nbl = E.level.GetBlock(changedWb.position + new HexXY(0, 1));
                                if (nbl != null && !changedBlocks.Contains(nbl))
                                    changedBlocks.Add(nbl);
                            }
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

    public void OnEnableLight(bool val)
    {
        if (val)
            sunLight.GetComponent<Light>().intensity = 1;
        else
            sunLight.GetComponent<Light>().intensity = 0f;
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
