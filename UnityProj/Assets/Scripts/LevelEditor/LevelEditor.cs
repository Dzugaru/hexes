using UnityEngine;
using System.Collections;
using Engine;
using System.Diagnostics;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

public class LevelEditor : MonoBehaviour
{
    public static LevelEditor S { get; private set; }
    
    Canvas canvas;
    Stack<IUndo> undos = new Stack<IUndo>(), redos = new Stack<IUndo>();
    Undos.TerrainPaint currentTerrainPaint;    

    public TerrainCellType? brushCellType;
    public int brushSize;
    public bool shouldPaintOnEmpty;
    public GameObject sunLight;
    public GameObject envRoot;

    public InputField levelNameField;

    

    void Awake()
    {
        S = this;

        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();                
    } 

    HexXY? brushOldMousePos;

    void Update()
    {
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

        HexXY p = getMouseOverTile();
        canvas.transform.Find("StatsPanel").transform.Find("CursorCoordsText").GetComponent<Text>().text = p.x + " " + p.y;

        if (Input.GetMouseButton(0) && brushCellType.HasValue)
        {
            if (p != brushOldMousePos)
            {
                if (currentTerrainPaint == null) currentTerrainPaint = new Undos.TerrainPaint();
                currentTerrainPaint.Paint(p, brushSize, shouldPaintOnEmpty, brushCellType.Value);
            }

            brushOldMousePos = p;
        }
        else
        {
            brushOldMousePos = null;
            if (currentTerrainPaint != null)
            {
                undos.Push(currentTerrainPaint);
                redos.Clear();
                currentTerrainPaint = null;
            }
        }



        //Undo and redo

        if (Input.GetKeyDown(KeyCode.BackQuote) && undos.Count > 0)
        {
            var op = undos.Pop();
            op.Undo();
            redos.Push(op);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1) && redos.Count > 0)
        {
            var op = redos.Pop();
            op.Redo();
            undos.Push(op);
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

    public void OnSave()
    {   
        using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(Path.Combine(Application.dataPath, "Resources/Levels/" + levelNameField.text + ".bytes"))))
        {
            Level.S.SaveStaticPart(writer);
            Level.S.SaveDynamicPart(writer);
        }

        AssetDatabase.Refresh();

        string envPrefabPath = "Assets/Resources/Prefabs/Env/" + levelNameField.text + ".prefab";
        var existingEnv = (GameObject)AssetDatabase.LoadAssetAtPath(envPrefabPath, typeof(GameObject));
        if (existingEnv == null)        
            PrefabUtility.CreatePrefab(envPrefabPath, envRoot);        
        else        
            PrefabUtility.ReplacePrefab(envRoot, existingEnv);

        AssetDatabase.SaveAssets();
    }

    public void OnLoad()
    {
        var levelDataAsset = (TextAsset)Resources.Load("Levels/" + levelNameField.text);
        if (levelDataAsset == null)
        {
            //TODO: level template?
            new Level();
            envRoot = new GameObject("Env");
            UnityEngine.Debug.Log("Created new level");
            return;
        }

        using (var reader = new BinaryReader(new MemoryStream(levelDataAsset.bytes)))
        {
            Level.Load(reader);
            Level.S.LoadDynamicPart(reader);
        }

        envRoot = Instantiate(Resources.Load<GameObject>("Prefabs/Env/" + levelNameField.text));
        envRoot.name = "Env";
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
