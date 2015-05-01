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

    Plane gamePlane = new Plane(new Vector3(0, 1, 0), 0);

    static readonly int grObjsClassesCount = Enum.GetValues(typeof(GrObjClass)).Length;

    static uint[] grObjsCountByClass;
    static Dictionary<D.GrObjHandle, GameObject> grObjs;


    void Awake()
    {
        grObjs = new Dictionary<D.GrObjHandle, GameObject>();
        grObjsCountByClass = new uint[grObjsClassesCount];

        if (g != null)
        {
            throw new System.InvalidProgramException("Two global objects");
        }

        g = this;

        D.setLogging(D.GetCallbackPointer((D.PtrToVoid)Log));
        D.SetCallback("showObjectOnTile", (D.HexXYShowObjectTypeFloatToVoid)ShowObjectOnTile);
        D.SetCallback("createGrObj", (D.TCreateGrObj)CreateGrObj);
        D.SetCallback("performOpOnGrObj", (D.TPerformOpOnGrObj)PerformOpOnGrObj);
        D.onStart();
      


        var wbhandle = D.queryWorld(); 

        GameObject ht = (GameObject)Instantiate(hexTerrainPrefab, Vector3.zero, Quaternion.identity);        
        ht.name = "Terrain Block 1";
        unsafe
        {
            ht.GetComponent<HexTerrain>().Generate(wbhandle.ToPointer());
        }
    }
  
    void Update()
    {   
        if (Input.GetMouseButtonDown(0))
        {
            float dist;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(gamePlane.Raycast(ray, out dist))
            {
                Vector3 inter = ray.origin + dist * ray.direction;
                Vector2 planePos = new Vector2(inter.x, inter.z);
                HexXY hexPos = HexXY.FromPlaneCoordinates(planePos);
                Debug.Log(hexPos);
            }
        }

        D.update(Time.deltaTime);        
    }

    static void Log(IntPtr msgPtr)
    {        
        Debug.Log(D.GetStringFromPointer(msgPtr));
    }

    static D.GrObjHandle CreateGrObj(GrObjClass objClass, GrObjType objType)
    {        
        string prefabPath = "Prefabs/" + objClass.ToString() + "/" + objType.ToString();
        GameObject obj = Instantiate((GameObject)Resources.Load(prefabPath));
        obj.SetActive(false);
        D.GrObjHandle handle = new D.GrObjHandle() { objClass = objClass, idx = grObjsCountByClass[(int)objClass]++ };
        grObjs.Add(handle, obj);
       
        return handle;
    }

    unsafe static void PerformOpOnGrObj(D.GrObjHandle objHandle, GrObjOperation op, IntPtr opParamsIntPtr)
    {   
        GameObject obj = grObjs[objHandle];
        switch (objHandle.objClass)
        {
            case GrObjClass.Entity: obj.GetComponent<EntityGraphics>().DispatchOp(op, opParamsIntPtr.ToPointer()); break;
        }       
    }

    static void ShowObjectOnTile(HexXY pos, ShowObjectType objType, float durSecs)
    {
        //Debug.Log(x + " " + y + " " + objName + " " + durSecs);
        string prefabPath = "Prefabs/";
        switch (objType)
        {
            case ShowObjectType.PathMarker: prefabPath += "Debug/PathMarker"; break;
            default: throw new InvalidProgramException();
        }

        GameObject obj = Instantiate((GameObject)Resources.Load(prefabPath));
        Vector2 planeCoord = pos.ToPlaneCoordinates();
        obj.transform.position = new Vector3(planeCoord.x, 0.2f, planeCoord.y);
        obj.GetComponent<IHasDuration>().SetDuration(durSecs);
    }
    
    //Bench for 100 100x100 world blocks was 0.44 sec
}

