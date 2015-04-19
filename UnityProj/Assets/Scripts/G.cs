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

    public void Awake()
    {
        if (g != null)
        {
            throw new System.InvalidProgramException("Two global objects");
        }

        g = this;       

     

        D.setLogger(D.GetCallbackPointer((D.PtrToVoid)(msgPtr =>
        {
            Debug.Log(D.GetStringFromPointer(msgPtr));
        })));

        D.start();
        var wbhandle = D.getWorldBlockHandle();        
        var worldBlock = (WorldBlock)Marshal.PtrToStructure(wbhandle, typeof(WorldBlock));

        GameObject ht = (GameObject)Instantiate(hexTerrainPrefab, Vector3.zero, Quaternion.identity);        
        ht.name = "Terrain Block 1";
        ht.GetComponent<HexTerrain>().Generate(worldBlock);


        
    }
    
    //Bench for 100 100x100 world blocks was 0.44 sec
}

