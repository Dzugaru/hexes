using UnityEngine;
using System.Collections;
using Engine;
using System.Diagnostics;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using System;

public class LevelEditor : MonoBehaviour
{
    public static LevelEditor S { get; private set; }
    
    Canvas canvas;
    Stack<IUndo> undos = new Stack<IUndo>(), redos = new Stack<IUndo>();
    Undos.TerrainPaint currentTerrainPaint;

    static readonly int entitiesClassesCount = Enum.GetValues(typeof(EntityClass)).Length;
    uint[] entitiesCountByClass = new uint[entitiesClassesCount];
    Dictionary<Interfacing.EntityHandle, GameObject> entities = new Dictionary<Interfacing.EntityHandle, GameObject>();

    public TerrainCellType? brushCellType;
    public int brushSize;
    public bool shouldPaintOnEmpty;
    public GameObject sunLight;
    public GameObject envRoot;

    public InputField levelNameField;

    public bool isInRuneDrawingMode = false;

    

    void Awake()
    {
        S = this;

        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();

        Interfacing.CreateEntity = CreateEntity;
      
        Interfacing.PerformInterfaceSpawn = PerformInterfaceSpawn;      
        Interfacing.PerformInterfaceDie = PerformInterfaceDie;
        Interfacing.PerformInterfaceUpdateRune = PerformInterfaceUpdateRune;        
    } 

    HexXY? brushOldMousePos;

    void Update()
    {
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

        HexXY p = getMouseOverTile();

        //Info
        canvas.transform.Find("StatsPanel").transform.Find("CursorCoordsText").GetComponent<Text>().text = p.x + " " + p.y;

        //Drawing terrain
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

        //Putting runes
        PutRunes(p);

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

    void PutRunes(HexXY p)
    {
        isInRuneDrawingMode = canvas.transform.Find("InstrPanel").transform.Find("Runes").GetComponent<StickyButton>().isPressed;
        if (!isInRuneDrawingMode) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            var existingRune = Level.S.GetEntities(p).OfType<Rune>().FirstOrDefault();
            if (existingRune != null)
            {
                var op = new Undos.RemoveEntity(existingRune, p);
                op.Remove();
                undos.Push(op);
                redos.Clear();
            }
        }
        else
            foreach (var kvp in PlayerInput.runeKeys)
                if (Input.GetKeyDown(kvp.Key))
                {
                    var existingRune = Level.S.GetEntities(p).OfType<Rune>().FirstOrDefault();
                    if (existingRune != null && existingRune.entityType != (uint)kvp.Value)
                    {
                        var op = new Undos.RemoveEntity(existingRune, p);
                        op.Remove();
                        undos.Push(op);
                        existingRune = null;
                    }

                    if (existingRune == null)
                    {
                        var rune = new Rune(kvp.Value, 0);
                        var op = new Undos.AddEntity(rune, p);
                        op.Add();
                        undos.Push(op);
                    }
                    else
                    {
                        var op = new Undos.RotateRune(p);
                        op.Rotate();
                        undos.Push(op);
                    }

                    redos.Clear();
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


    Interfacing.EntityHandle CreateEntity(EntityClass objClass, uint objType)
    {
        string prefabPath = "Prefabs/" + objClass.ToString() + "/";
        switch (objClass)
        {
            case EntityClass.Character: prefabPath += ((CharacterType)objType).ToString(); break;
            case EntityClass.Rune: prefabPath += ((RuneType)objType).ToString(); break;
            case EntityClass.Collectible: prefabPath += ((CollectibleType)objType).ToString(); break;
            case EntityClass.SpellEffect: prefabPath += ((SpellEffectType)objType).ToString(); break;
        }

        GameObject obj = Instantiate((GameObject)Resources.Load(prefabPath));
        obj.SetActive(false);

        var handle = new Interfacing.EntityHandle() { objClass = objClass, idx = entitiesCountByClass[(int)objClass]++ };

        var objGr = obj.GetComponent<EntityGraphics>();
        objGr.entityType = objType;
        objGr.entityHandle = handle;

        entities.Add(handle, obj);

        return handle;
    }

    void PerformInterfaceSpawn(Interfacing.EntityHandle objHandle, HexXY pos)
    {
        GameObject obj = entities[objHandle];
        obj.GetComponent<EntityGraphics>().Spawn(pos);
    }

    void PerformInterfaceDie(Interfacing.EntityHandle objHandle)
    {
        GameObject obj = entities[objHandle];
        obj.GetComponent<EntityGraphics>().Die();
    }

    void PerformInterfaceUpdateRune(Interfacing.EntityHandle objHandle, uint dir)
    {
        GameObject obj = entities[objHandle];
        obj.GetComponent<RuneGraphics>().UpdateInterface(dir);
    }
}
