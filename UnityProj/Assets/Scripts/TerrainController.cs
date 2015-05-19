using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TerrainController : MonoBehaviour
{
    public static TerrainController S { get; private set; }

    static readonly Plane gamePlane = new Plane(new Vector3(0, 1, 0), 0);
    Dictionary<HexXY, HexTerrain> hexTerrains = new Dictionary<HexXY, HexTerrain>();

    public GameObject hexTerrainPrefab;
    public int createRadius, destroyRadius;
    public float hexInset = 0.95f;
    public float terrainTexScale = 0.2f;

    public bool isWallsEnabled;

    public bool IsWallsEnabled
    {
        get
        {
            return isWallsEnabled;
        }

        set
        {
            if (isWallsEnabled != value)
            {
                isWallsEnabled = value;
                foreach (var t in hexTerrains.Values)
                    t.enableWalls = isWallsEnabled;
            }
        }
    }

    void Awake()
    {
        S = this;
    }

    void Update()
    {
        if (Level.S == null) return;

        Ray viewRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        float dist;
        gamePlane.Raycast(viewRay, out dist);
        Vector3 coord3d = viewRay.origin + viewRay.direction * dist;
        Vector2 coord2d = new Vector2(coord3d.x, coord3d.z);

        HexXY centerBlockPos = Level.GetBlockCoords(coord2d);

        //Destroy out of range
        List<HexXY> destroyList = new List<HexXY>();
        foreach (var p in hexTerrains.Keys)        
            if (HexXY.Dist(centerBlockPos, p) > destroyRadius)            
                destroyList.Add(p);

        foreach (var p in destroyList)
        {
            Destroy(hexTerrains[p].gameObject);
            hexTerrains.Remove(p);
            Debug.Log("Terrain " + p + " destroyed");
        }

        //Create if in range
        for (int x = -createRadius; x <= createRadius; x++)
        {
            for (int y = -createRadius; y <= createRadius; y++)
            {
                var p = centerBlockPos + new HexXY(x, y);
                if (!hexTerrains.ContainsKey(p))
                {
                    var wb = Level.S.GetBlock(p);
                    if (wb != null)
                    {
                        hexTerrains.Add(p, CreateHexTerrain(wb));
                        Debug.Log("Terrain " + p + " created");
                    }
                }
            }
        }
    }

    HexTerrain CreateHexTerrain(WorldBlock block)
    {
        var tObj = Instantiate(hexTerrainPrefab);
        tObj.transform.SetParent(transform, false);
        tObj.name = block.position.ToString();
        Vector2 centerp = new HexXY(block.position.x * WorldBlock.sz, block.position.y * WorldBlock.sz).ToPlaneCoordinates();
        tObj.transform.position = new Vector3(centerp.x, 0, centerp.y);

        var t = tObj.GetComponent<HexTerrain>();
        t.enableWalls = isWallsEnabled;
        t.GenerateMultiple(block, hexInset, terrainTexScale);

        return t;
    }

    public void RecreateHexTerrain(WorldBlock block)
    {
        if (hexTerrains.ContainsKey(block.position))        
            Destroy(hexTerrains[block.position].gameObject);
        else
            Debug.Log("Terrain " + block.position + " created");

        hexTerrains[block.position] = CreateHexTerrain(block);
    }
}

