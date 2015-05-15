﻿using System;
using UnityEngine;
using Engine;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;

public class G : MonoBehaviour
{    
    public static G g { get; private set; }

    public GameObject hexTerrainPrefab;

    //Some settings
    public float hexInset = 0.9f;
    public float terrainTexScale = 0.1f;
    public float terrainFullness = 0.75f;
    public float terrainSnowiness = 0.1f;

    public static readonly Plane gamePlane = new Plane(new Vector3(0, 1, 0), 0);

    static readonly int entitiesClassesCount = Enum.GetValues(typeof(EntityClass)).Length;

    static uint[] entitiesCountByClass;
    static Dictionary<Interfacing.EntityHandle, GameObject> entities;

    public static bool isTimeStopped;

    public float timeStopMultiplier;


    void Awake()
    {
        entities = new Dictionary<Interfacing.EntityHandle, GameObject>();
        entitiesCountByClass = new uint[entitiesClassesCount];

        if (g != null)
        {
            throw new InvalidProgramException("Two global objects");
        }

        g = this;

        Logger.LogAction = msg => Debug.Log(msg);

        Interfacing.CreateEntity = CreateEntity;
        Interfacing.PerformInterfaceAttack = PerformInterfaceAttack;
        Interfacing.PerformInterfaceSpawn = PerformInterfaceSpawn;
        Interfacing.PerformInterfaceMove = PerformInterfaceMove;
        Interfacing.PerformInterfaceDamage = PerformInterfaceDamage;
        Interfacing.PerformInterfaceDie = PerformInterfaceDie;
        Interfacing.PerformInterfaceStop = PerformInterfaceStop;

        Interfacing.PerformInterfaceUpdateHP = PerformInterfaceUpdateHP;
        Interfacing.PerformInterfaceUpdateRune = PerformInterfaceUpdateRune;

        E.Start();


        GameObject ht = (GameObject)Instantiate(hexTerrainPrefab, Vector3.zero, Quaternion.identity);        
        ht.name = "Terrain";       
        ht.GetComponent<HexTerrain>().Generate(WorldBlock.S);

        //DEBUG load saved spell
        string spellFilePath = Path.Combine(Application.persistentDataPath, "spell");
        if (File.Exists(spellFilePath))
        {
            //Debug.Log(spellFilePath);
            using (var writer = new BinaryReader(File.OpenRead(spellFilePath)))
                E.player.currentSpell = Spell.Load(writer);
        }
    }

    void Update()
    {
        if (isTimeStopped)        
            Time.timeScale = timeStopMultiplier;        
        else        
            Time.timeScale = 1;

        E.Update(Time.deltaTime);
    }

    static Interfacing.EntityHandle CreateEntity(EntityClass objClass, uint objType)
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

    static void PerformInterfaceAttack(Interfacing.EntityHandle objHandle, HexXY pos)
    {        
        GameObject obj = entities[objHandle];       
        obj.GetComponent<EntityGraphics>().Attack(pos);
    }

    static void PerformInterfaceSpawn(Interfacing.EntityHandle objHandle, HexXY pos)
    {
        GameObject obj = entities[objHandle];
        obj.GetComponent<EntityGraphics>().Spawn(pos);
    }

    static void PerformInterfaceMove(Interfacing.EntityHandle objHandle, HexXY pos, float timeToGetThere)
    {
        GameObject obj = entities[objHandle];
        obj.GetComponent<EntityGraphics>().Move(pos, timeToGetThere);
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

    static void PerformInterfaceUpdateRune(Interfacing.EntityHandle objHandle, uint dir)
    {
        GameObject obj = entities[objHandle];
        obj.GetComponent<RuneGraphics>().UpdateInterface(dir);
    }

    //D engine

    //void Update()
    //{
    //    if (isTimeStopped)
    //    {
    //        Time.timeScale = timeStopMultiplier;
    //    }
    //    else
    //    {
    //        Time.timeScale = 1;
    //    }
    //    //if(!isTimeStopped)
    //        D.update(Time.deltaTime);        
    //}

    //static void Log(IntPtr msgPtr)
    //{        
    //    Debug.Log(D.GetStringFromPointer(msgPtr));
    //}

    //static D.EntityHandle CreateEntity(EntityClass objClass, int objType)
    //{
    //    string prefabPath = "Prefabs/" + objClass.ToString() + "/";
    //    switch (objClass)
    //    {
    //        case EntityClass.Character: prefabPath += ((CharacterType)objType).ToString(); break;
    //        case EntityClass.Rune: prefabPath += ((RuneType)objType).ToString(); break;
    //        case EntityClass.Collectible: prefabPath += ((CollectibleType)objType).ToString(); break;
    //    }

    //    GameObject obj = Instantiate((GameObject)Resources.Load(prefabPath));
    //    obj.SetActive(false);

    //    D.EntityHandle handle = new D.EntityHandle() { objClass = objClass, idx = entitiesCountByClass[(int)objClass]++ };

    //    var objGr = obj.GetComponent<EntityGraphics>();
    //    objGr.entityType = objType;
    //    objGr.entityHandle = handle;

    //    entities.Add(handle, obj);

    //    return handle;
    //}

    //unsafe static void PerformOpOnEntity(D.EntityHandle objHandle, EntityOperation op, void* args)
    //{        
    //    GameObject obj = entities[objHandle];       
    //    obj.GetComponent<EntityGraphics>().DispatchOp(op, args);                
    //}

    //static void ShowEffectOnTile(HexXY pos, EffectType effectType)
    //{        
    //    string prefabPath = "Prefabs/Effects/" + effectType.ToString();        

    //    GameObject obj = Instantiate((GameObject)Resources.Load(prefabPath));
    //    Vector2 planeCoord = pos.ToPlaneCoordinates();
    //    obj.transform.position = new Vector3(planeCoord.x, 0.2f, planeCoord.y);
    //    //obj.GetComponent<IHasDuration>().SetDuration(durSecs);
    //}

    //static void StopOrResumeTime(bool isStop)
    //{
    //    isTimeStopped = isStop;
    //}

    //static void DInit()
    //{
    //    D.setLogging(D.GetCallbackPointer((D.PtrToVoid)Log));
    //    D.SetCallback("showEffectOnTile", (D.TShowEffectOnTile)ShowEffectOnTile);
    //    D.SetCallback("createEntity", (D.TCreateEntity)CreateEntity);
    //    D.SetCallback("performOpOnEntity", (D.TPerformOpOnEntity)PerformOpOnEntity);
    //    D.SetCallback("stopOrResumeTime", (D.BoolToVoid)StopOrResumeTime);
    //    D.onStart();

    //    var wbhandle = D.queryWorld();
    //}

    //Bench for 100 100x100 world blocks was 0.44 sec
}

