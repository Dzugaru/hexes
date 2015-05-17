using UnityEngine;
using System.Collections;
using Engine;
using System.Diagnostics;
using UnityEngine.UI;

public class LevelEditor : MonoBehaviour
{
    public static LevelEditor S { get; private set; }

    public GameObject hexTerrainPrefab;
    public float hexInset = 0.95f;
    public float terrainTexScale = 0.2f;

    GameObject terrain;
    Canvas canvas;

    public TerrainCellType brushCellType;
    public int brushSize;
    public bool shouldPaintOnEmpty;

    void Start ()
    {
        S = this;

        WorldBlock.S = new WorldBlock(new HexXY(0, 0));
        WorldBlock.S.Generate(new BinaryNoiseFunc(new Vector2(100, 200), 0.25f, 0.7f),
                            new BinaryNoiseFunc(new Vector2(200, 100), 0.25f, 0.3f), true);

        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();

        RegenerateTerrain();
    }

    void RegenerateTerrain()
    {
        Stopwatch sw = new Stopwatch();

        sw.Start();
        bool wasWallsEnabled = false;
        if (terrain != null)
        {
            wasWallsEnabled = terrain.GetComponent<HexTerrain>().enableWalls;
            Destroy(terrain);
        }

        terrain = (GameObject)Instantiate(hexTerrainPrefab, Vector3.zero, Quaternion.identity);
        terrain.GetComponent<HexTerrain>().enableWalls = wasWallsEnabled;
        terrain.GetComponent<HexTerrain>().GenerateMultiple(WorldBlock.S, hexInset, terrainTexScale);        
        sw.Stop();
        UnityEngine.Debug.Log("Terrain regen in " + sw.ElapsedMilliseconds + " ms");
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
                bool isChanged = false;

                for (int x = -(brushSize - 1); x <= brushSize - 1; x++)
                {
                    for (int y = -(brushSize - 1); y <= brushSize - 1; y++)
                    {
                        HexXY d = new HexXY(x, y);
                        if (HexXY.Dist(new HexXY(0, 0), d) > brushSize - 1) continue;
                        HexXY pd = p + d;                       

                        if (pd.x >= 1 && pd.x < WorldBlock.sz - 1 && pd.y >= 1 && pd.y < WorldBlock.sz - 1)
                        {
                            if (WorldBlock.S.cellTypes[pd.x, pd.y] != brushCellType &&
                                (shouldPaintOnEmpty || WorldBlock.S.cellTypes[pd.x, pd.y] != TerrainCellType.Empty || brushCellType == TerrainCellType.Empty))
                            {
                                WorldBlock.S.cellTypes[pd.x, pd.y] = brushCellType;
                                isChanged = true;
                            }
                        }
                    }
                }

                if(isChanged)
                    RegenerateTerrain();
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
        terrain.GetComponent<HexTerrain>().enableWalls = val;
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
