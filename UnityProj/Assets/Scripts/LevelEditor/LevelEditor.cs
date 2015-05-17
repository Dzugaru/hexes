using UnityEngine;
using System.Collections;
using Engine;

public class LevelEditor : MonoBehaviour
{
    public GameObject hexTerrainPrefab;
    public float hexInset = 0.95f;
    public float terrainTexScale = 0.2f;

    void Start ()
    {
        WorldBlock.S = new WorldBlock(new HexXY(0, 0));
        WorldBlock.S.Generate(new BinaryNoiseFunc(new Vector2(100, 200), 0.25f, 0.7f),
                            new BinaryNoiseFunc(new Vector2(200, 100), 0.25f, 0.3f), true);

        GameObject ht = (GameObject)Instantiate(hexTerrainPrefab, Vector3.zero, Quaternion.identity);
        ht.name = "Terrain";
        ht.GetComponent<HexTerrain>().GenerateMultiple(WorldBlock.S, hexInset, terrainTexScale);
    }
	
	
	void Update ()
    {
	
	}
}
