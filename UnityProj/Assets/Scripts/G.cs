using UnityEngine;
using Engine;

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

        world = new World();
        var worldBlock = new WorldBlock(new HexXY(0, 0), 10);

        var nonEmpty = new BinaryNoiseFunc(new Vector2(100, 200), 0.25f, terrainFullness);
        var snow = new BinaryNoiseFunc(new Vector2(200, 100), 0.25f, terrainSnowiness);

        worldBlock.Generate(nonEmpty, snow);

        GameObject ht = (GameObject)Instantiate(hexTerrainPrefab, Vector3.zero, Quaternion.identity);
        //ht.transform.position = new Vector3(1, 1, 1);
        ht.name = "Terrain Block 1";
        ht.GetComponent<HexTerrain>().Generate(worldBlock);    

        Debug.Log(D.sqr(9));
        Debug.Log(D.dbl(2));
        Debug.Log(D.five());
    }
}

