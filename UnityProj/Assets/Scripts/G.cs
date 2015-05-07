using System;
using UnityEngine;
using Engine;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class G : MonoBehaviour
{
    public static World world;
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
    static Dictionary<D.EntityHandle, GameObject> entities;

    public static bool isTimeStopped;

    public float timeStopMultiplier;


    unsafe void Awake()
    {
        entities = new Dictionary<D.EntityHandle, GameObject>();
        entitiesCountByClass = new uint[entitiesClassesCount];

        if (g != null)
        {
            throw new System.InvalidProgramException("Two global objects");
        }

        g = this;

        D.setLogging(D.GetCallbackPointer((D.PtrToVoid)Log));
        D.SetCallback("showEffectOnTile", (D.TShowEffectOnTile)ShowEffectOnTile);
        D.SetCallback("createEntity", (D.TCreateEntity)CreateEntity);
        D.SetCallback("performOpOnEntity", (D.TPerformOpOnEntity)PerformOpOnEntity);
        D.SetCallback("stopOrResumeTime", (D.BoolToVoid)StopOrResumeTime);
        D.onStart();

        var wbhandle = D.queryWorld(); 

        GameObject ht = (GameObject)Instantiate(hexTerrainPrefab, Vector3.zero, Quaternion.identity);        
        ht.name = "Terrain Block 1";       
        ht.GetComponent<HexTerrain>().Generate(wbhandle);        
    }
  
    void Update()
    {
        if (isTimeStopped)
        {
            Time.timeScale = timeStopMultiplier;
        }
        else
        {
            Time.timeScale = 1;
        }
        //if(!isTimeStopped)
            D.update(Time.deltaTime);        
    }

    static void Log(IntPtr msgPtr)
    {        
        Debug.Log(D.GetStringFromPointer(msgPtr));
    }

    static D.EntityHandle CreateEntity(EntityClass objClass, int objType)
    {
        string prefabPath = "Prefabs/" + objClass.ToString() + "/";
        switch (objClass)
        {
            case EntityClass.Character: prefabPath += ((CharacterType)objType).ToString(); break;
            case EntityClass.Inanimate: prefabPath += ((InanimateType)objType).ToString(); break;
            case EntityClass.Collectible: prefabPath += ((CollectibleType)objType).ToString(); break;
        }
        
        GameObject obj = Instantiate((GameObject)Resources.Load(prefabPath));
        obj.SetActive(false);
        
        D.EntityHandle handle = new D.EntityHandle() { objClass = objClass, idx = entitiesCountByClass[(int)objClass]++ };
      
        var objGr = obj.GetComponent<EntityGraphics>();
        objGr.entityType = objType;
        objGr.entityHandle = handle;
        
        entities.Add(handle, obj);
       
        return handle;
    }

    unsafe static void PerformOpOnEntity(D.EntityHandle objHandle, EntityOperation op, void* args)
    {        
        GameObject obj = entities[objHandle];
        switch (objHandle.objClass)
        {
            case EntityClass.Character:
                obj.GetComponent<CharacterGraphics>().DispatchOp(op, args);
                break;

            case EntityClass.Collectible:
            case EntityClass.Inanimate:
                obj.GetComponent<InanimateGraphics>().DispatchOp(op, args);
                break;            
        }       
    }

    static void ShowEffectOnTile(HexXY pos, EffectType effectType)
    {        
        string prefabPath = "Prefabs/Effects/" + effectType.ToString();        

        GameObject obj = Instantiate((GameObject)Resources.Load(prefabPath));
        Vector2 planeCoord = pos.ToPlaneCoordinates();
        obj.transform.position = new Vector3(planeCoord.x, 0.2f, planeCoord.y);
        //obj.GetComponent<IHasDuration>().SetDuration(durSecs);
    }

    static void StopOrResumeTime(bool isStop)
    {
        isTimeStopped = isStop;
    }
    
    //Bench for 100 100x100 world blocks was 0.44 sec
}

