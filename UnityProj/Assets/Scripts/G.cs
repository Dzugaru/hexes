using System;
using UnityEngine;
using Engine;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;

public class G : MonoBehaviour
{    
    public static G S { get; private set; }

    //Some settings
    public float hexInset = 0.975f;
    public float terrainTexScale = 0.1f;
    public float terrainFullness = 0.75f;
    public float terrainSnowiness = 0.1f;

    public static readonly Plane gamePlane = new Plane(new Vector3(0, 1, 0), 0);
   
    
    static Dictionary<Interfacing.EntityHandle, GameObject> entities;

    public static bool isTimeStopped;    

    public float timeStopMultiplier;

    [SerializeField]
    GameObject freeList;

    public bool isEditMode;


    void Awake()
    {
        if (S != null)
        {
            throw new InvalidProgramException("Two global objects");
        }

        S = this;

        if (isEditMode)
        {
            GoToEditMode();
            return;
        }

        entities = new Dictionary<Interfacing.EntityHandle, GameObject>();

        Logger.LogAction = msg => Debug.Log(msg);

        Interfacing.CreateEntity = CreateEntity;
        Interfacing.PerformInterfaceAttack = PerformInterfaceAttack;
        Interfacing.PerformInterfaceSpawn = PerformInterfaceSpawn;
        Interfacing.PerformInterfaceMove = PerformInterfaceMove;
        Interfacing.PerformInterfaceMovePrecise = PerformInterfaceMovePrecise;  
        Interfacing.PerformInterfaceDamage = PerformInterfaceDamage;
        Interfacing.PerformInterfaceDie = PerformInterfaceDie;
        Interfacing.PerformInterfaceStop = PerformInterfaceStop;

        Interfacing.PerformInterfaceUpdateHP = PerformInterfaceUpdateHP;
        Interfacing.PerformInterfaceUpdateRotation = PerformInterfaceUpdateRotation;        


        E.LoadLevel(Application.loadedLevelName);       


        //DEBUG load saved spell
        string spellFilePath = Path.Combine(Application.persistentDataPath, "spell");
        if (File.Exists(spellFilePath))
        {
            //Debug.Log(spellFilePath);
            using (var writer = new BinaryReader(File.OpenRead(spellFilePath)))
                E.player.currentSpell = Spell.Load(writer);
        }
    }

   

    void Start()
    {
        Camera.main.GetComponent<CameraControl>().SetOnPlayerInstantly();
    }

    void Update()
    {
        if (isTimeStopped)        
            Time.timeScale = timeStopMultiplier;        
        else        
            Time.timeScale = 1;

        E.Update(Time.deltaTime);
    }

    static Interfacing.EntityHandle CreateEntity(Entity ent)
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
        obj.transform.SetParent(GameObject.Find("Entities").transform, false);
        obj.SetActive(false);
        

        var handle = new Interfacing.EntityHandle() {  idx = (uint)entities.Count };

        var objGr = obj.GetComponent<EntityGraphics>();
        objGr.entity = ent;
        objGr.entityType = ent.entityType;        

        entities.Add(handle, obj);

        return handle;
    }

    static void PerformInterfaceAttack(Interfacing.EntityHandle objHandle, HexXY pos)
    {        
        GameObject obj = entities[objHandle];       
        obj.GetComponent<EntityGraphics>().Attack(pos);
    }

    static void PerformInterfaceSpawn(Interfacing.EntityHandle objHandle, HexXY pos, uint dir)
    {
        GameObject obj = entities[objHandle];
        obj.GetComponent<EntityGraphics>().Spawn(pos, dir);
    }

    static void PerformInterfaceMove(Interfacing.EntityHandle objHandle, HexXY pos, float timeToGetThere)
    {
        GameObject obj = entities[objHandle];
        obj.GetComponent<EntityGraphics>().Move(pos, timeToGetThere);
    }

    static void PerformInterfaceMovePrecise(Interfacing.EntityHandle objHandle, Vector2 pos, float timeToGetThere)
    {
        GameObject obj = entities[objHandle];
        obj.GetComponent<EntityGraphics>().MovePrecise(pos, timeToGetThere);
    }

    static void PerformInterfaceStop(Interfacing.EntityHandle objHandle, HexXY pos)
    {
        GameObject obj = entities[objHandle];
        obj.GetComponent<EntityGraphics>().Stop(pos);
    }

    static void PerformInterfaceDamage(Interfacing.EntityHandle objHandle, float dmg)
    {
        GameObject obj = entities[objHandle];
        obj.GetComponent<EntityGraphics>().Damage(dmg);
    }

    static void PerformInterfaceDie(Interfacing.EntityHandle objHandle)
    {
        GameObject obj = entities[objHandle];
        obj.GetComponent<EntityGraphics>().Die();
    }

    static void PerformInterfaceUpdateHP(Interfacing.EntityHandle objHandle, float currentHP, float maxHP)
    {
        GameObject obj = entities[objHandle];
        obj.GetComponent<EntityGraphics>().UpdateHP(currentHP, maxHP);
    }

    static void PerformInterfaceUpdateRotation(Interfacing.EntityHandle objHandle, uint dir)
    {
        GameObject obj = entities[objHandle];
        obj.GetComponent<EntityGraphics>().UpdateInterfaceRotation(dir);
    }

    public static bool IsInUnityEditMode()
    {
#if UNITY_EDITOR
        return !UnityEditor.EditorApplication.isPlaying;
#else
        return false;
#endif 
    }

    Dictionary<HexXY, GameObject> debugCells = new Dictionary<HexXY, GameObject>();
    public void DebugShowCell(HexXY p)
    {
        var cell = Instantiate(Resources.Load<GameObject>("Prefabs/Editor/SBTileVis"));
        debugCells.Add(p, cell);        
        Vector2 fracPos = p.ToPlaneCoordinates();
        cell.transform.position = new Vector3(fracPos.x, 0.01f, fracPos.y);
    }

    public void DebugHideCell(HexXY p)
    {        
        var cell = debugCells[p];
        debugCells.Remove(p);
        Destroy(cell);        
    }

    void GoToEditMode()
    {
        Destroy(GameObject.Find("UICamera"));
        Destroy(GameObject.Find("Main Camera"));
        //Destroy(GameObject.Find("Sun"));
        Destroy(GameObject.Find("UI"));
        Destroy(GameObject.Find("EventSystem"));       
        Destroy(GameObject.Find("Entities"));
        Destroy(freeList);

        Destroy(gameObject);

        Application.LoadLevelAdditive("editor");
    }
}

