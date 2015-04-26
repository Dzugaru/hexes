using System;
using UnityEngine;
using Engine;
using System.Runtime.InteropServices;

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

    void Awake()
    {
        if (g != null)
        {
            throw new System.InvalidProgramException("Two global objects");
        }

        g = this;

        D.setLogging(D.GetCallbackPointer((D.PtrToVoid)(msgPtr =>
        {
            Debug.Log(D.GetStringFromPointer(msgPtr));
        })));
        D.SetCallback("showObjectOnTile", (D.HexXYShowObjectTypeFloatToVoid)ShowObjectOnTile);
        D.onStart();


        var wbhandle = D.queryWorld(); 

        GameObject ht = (GameObject)Instantiate(hexTerrainPrefab, Vector3.zero, Quaternion.identity);        
        ht.name = "Terrain Block 1";
        unsafe
        {
            ht.GetComponent<HexTerrain>().Generate(wbhandle.ToPointer());
        }
    }

    bool wasFromChosen = false;
    HexXY from;

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
                if (!wasFromChosen)
                {
                    from = hexPos;
                }
                else
                {
                    D.calcAndShowPath(from, hexPos);
                }

                wasFromChosen = !wasFromChosen;
            }
        }
    }

    

    void ShowObjectOnTile(HexXY pos, ShowObjectType objType, float durSecs)
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

