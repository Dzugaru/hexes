﻿#if UNITY_EDITOR

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

   
    
    Dictionary<Interfacing.EntityHandle, GameObject> entities = new Dictionary<Interfacing.EntityHandle, GameObject>();

    public TerrainCellType? brushCellType;
    public int brushSize;
    public bool shouldPaintOnEmpty;
    public GameObject sunLight;
    public GameObject envRoot;

    public InputField levelNameField;
    public GameObject sbvisParent;

    public bool isInRuneDrawingMode = false;
    public bool isInPassabilityMode = false;
    public bool isInEntitySetMode = false;

    public LevelScript levelScript;
    public Type levelScriptIDs;

    void Awake()
    {
        S = this;

        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();

        Interfacing.CreateEntity = CreateEntity;
      
        Interfacing.PerformInterfaceSpawn = PerformInterfaceSpawn;      
        Interfacing.PerformInterfaceDie = PerformInterfaceDie;
        Interfacing.PerformInterfaceUpdateRotation = PerformInterfaceUpdateRotation;

        var instrPanel = canvas.transform.Find("InstrPanel");

        instrPanel.Find("Passability").GetComponent<StickyButton>().PressedChanged += OnPassabilityModeChanged;
        instrPanel.Find("EntitySet").GetComponent<StickyButton>().PressedChanged += val => isInEntitySetMode = val;
        instrPanel.Find("Runes").GetComponent<StickyButton>().PressedChanged += val => isInRuneDrawingMode = val;


        sbvisParent = GameObject.Find("SBTileVis");
    }

   

    HexXY? brushOldMousePos;

    void Update()
    {
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

        HexXY p = getMouseOverTile();

        //Info
        canvas.transform.Find("StatsPanel").Find("CursorCoordsText").GetComponent<Text>().text = p.x + " " + p.y;

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

        //Putting entities
        PutEntities(p);

        //Changing passability
        if (isInPassabilityMode && Input.GetMouseButtonDown(0))
            ChangeStaticPassability(p, true); 

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
                        redos.Clear();
                    }

                    if (existingRune == null)
                    {
                        var rune = new Rune(kvp.Value, 0);
                        var op = new Undos.AddEntity(rune, p);
                        op.Add();
                        undos.Push(op);
                        redos.Clear();
                    }
                    else if(existingRune.CanRotate)
                    {
                        var op = new Undos.RotateEntity(existingRune, p);
                        op.Rotate();
                        undos.Push(op);
                        redos.Clear();
                    }                    
                }
    }

    void PutEntities(HexXY p)
    {
        if (!isInEntitySetMode) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                var ent = Level.S.GetEntities(p).FirstOrDefault();
                if (ent != null && ent is IRotatable)
                {
                    var op = new Undos.RotateEntity(ent, p);
                    op.Rotate();
                    undos.Push(op);
                    redos.Clear();
                }
            }
            else
            {
                var existingEntity = Level.S.GetEntities(p).FirstOrDefault();
                if (existingEntity != null)
                {                    
                    Selection.activeGameObject = entities[existingEntity.graphicsHandle];
                }
                else //TODO: make possible stacking entities with Alt or something?
                {
                    var prefab = Selection.activeGameObject;
                    if (prefab == null || PrefabUtility.GetPrefabType(prefab) == PrefabType.None) return;
                    var creator = prefab.GetComponent<EditorEntityBacklink>();
                    if (creator == null) return;
                    var ent = creator.CreateEntity();
                    var op = new Undos.AddEntity(ent, p);

                    op.Add();
                    undos.Push(op);
                    redos.Clear();
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            var ent = Level.S.GetEntities(p).FirstOrDefault();
            if (ent != null)
            {
                var op = new Undos.RemoveEntity(ent, p);
                op.Remove();
                undos.Push(op);
                redos.Clear();
            }
        }
    }

    public void ChangeStaticPassability(HexXY p, bool isUndoable)
    {
        var op = new Undos.ChangePassability(p);
        if (op.Change() && isUndoable)
        {
            undos.Push(op);
            redos.Clear();
        }
    }
    
    void OnPassabilityModeChanged(bool isEnabled)
    {
        isInPassabilityMode = isEnabled;        
        sbvisParent.SetActive(isInPassabilityMode);         
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
        string filePath = Path.Combine(Application.dataPath, "Resources/Levels/" + levelNameField.text + ".bytes");
        if (File.Exists(filePath))        
            File.Copy(filePath, filePath + ".bak", true);        

        using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(filePath)))
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

        if (envRoot != null) Destroy(envRoot);
        envRoot = Instantiate(Resources.Load<GameObject>("Prefabs/Env/" + levelNameField.text));
        envRoot.name = "Env";

        //SB vis
        foreach (var wb in Level.S.GetAllBlocks())
            for (int x = 0; x < WorldBlock.sz; x++)
                for (int y = 0; y < WorldBlock.sz; y++)
                {
                    if (wb.GetCellType(new HexXY(x, y)) != TerrainCellType.Empty &&
                        wb.pfBlockedMap[x, y] == WorldBlock.PFBlockType.StaticBlocked)
                    {
                        var p = new HexXY(wb.position.x * WorldBlock.sz + x, wb.position.y * WorldBlock.sz + y);
                        var tileVis = Instantiate(Resources.Load<GameObject>("Prefabs/Editor/SBTileVis"));
                        tileVis.GetComponent<StaticBlockedTileVisual>().p = p;
                        tileVis.transform.SetParent(sbvisParent.transform, false);
                        var pp = p.ToPlaneCoordinates();
                        tileVis.transform.localPosition = new Vector3(pp.x, 0, pp.y);
                    }
                }

        levelScript = (LevelScript)Activator.CreateInstance(System.Reflection.Assembly.GetExecutingAssembly()
            .GetTypes().First(t => t.Namespace == "Engine.LevelScripts" && t.Name == levelNameField.text));

        levelScriptIDs = levelScript.GetType().GetNestedType("ID");
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


    Interfacing.EntityHandle CreateEntity(Entity ent)
    {
        string prefabPath = "Prefabs/" + ent.entityClass.ToString() + "/";
        switch (ent.entityClass)
        {
            case EntityClass.Character: prefabPath += ((CharacterType)ent.entityType).ToString(); break;
            case EntityClass.Rune: prefabPath += ((RuneType)ent.entityType).ToString(); break;
            case EntityClass.Collectible: prefabPath += ((CollectibleType)ent.entityType).ToString(); break;
            case EntityClass.SpellEffect: prefabPath += ((SpellEffectType)ent.entityType).ToString(); break;
            case EntityClass.Mech: prefabPath += ((MechType)ent.entityType).ToString(); break;
        }

        GameObject obj = Instantiate((GameObject)Resources.Load(prefabPath));
        obj.SetActive(false);
        obj.transform.SetParent(GameObject.Find("Entities").transform, false);

        var handle = new Interfacing.EntityHandle() { idx = (uint)entities.Count };

        var objGr = obj.GetComponent<EntityGraphics>();
        objGr.entity = ent;
        objGr.entityType = ent.entityType;        

        entities.Add(handle, obj);       

        return handle;
    }

    void PerformInterfaceSpawn(Interfacing.EntityHandle objHandle, HexXY pos, uint dir)
    {
        GameObject obj = entities[objHandle];
        obj.GetComponent<EntityGraphics>().Spawn(pos, dir);
    }

    void PerformInterfaceDie(Interfacing.EntityHandle objHandle)
    {
        GameObject obj = entities[objHandle];
        obj.GetComponent<EntityGraphics>().Die();
    }

    void PerformInterfaceUpdateRotation(Interfacing.EntityHandle objHandle, uint dir)
    {
        GameObject obj = entities[objHandle];
        obj.GetComponent<EntityGraphics>().UpdateInterfaceRotation(dir);
    }
}
#endif