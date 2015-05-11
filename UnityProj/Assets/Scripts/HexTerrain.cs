using UnityEngine;
using Engine;
using System;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexTerrain : MonoBehaviour
{
    static readonly float r3 = Mathf.Sqrt(3);

    Mesh mesh;

    static Vector2[] cellVertices = new Vector2[]
        {            
            new Vector2(0.5f / r3, 0.5f),
            new Vector2(1f / r3, 0),
            new Vector2(0.5f / r3, -0.5f),
            new Vector2(-0.5f / r3, -0.5f),
            new Vector2(-1f / r3, 0),
            new Vector2(-0.5f / r3, 0.5f),
        };

    static int[] cellTriangles = new int[]
        {
            0, 1, 2,//Right triangle
            2, 3, 4,//Bottom-left triangle
            4, 5, 0,//Upper-left triangle
            0, 2, 4//Center triangle
        };

    unsafe public void Generate(void* pBlock)
    {
        int* cellTypes = (int*)pBlock + 2;
        int* cellTypeCounts = cellTypes + World.BlockSize * World.BlockSize;
        int nonEmptyCellsCount = *(cellTypeCounts + World.TerrainCellTypesCount);

        mesh = new Mesh();
        mesh.name = "Hex Terrain mesh";
        GetComponent<MeshFilter>().mesh = mesh;

        var mats = new Material[2];
        mats[0] = (Material)Resources.Load("Terrain/DryGround");
        mats[1] = (Material)Resources.Load("Terrain/Snow");        
        GetComponent<MeshRenderer>().sharedMaterials = mats;

        int typescnt = World.TerrainCellTypesCount - 1;
        int cellvcnt = cellVertices.Length;
        int celltcnt = cellTriangles.Length;
        int vcount = nonEmptyCellsCount * cellvcnt;        
        Vector3[] vertices = new Vector3[vcount];
        Vector3[] normals = new Vector3[vcount];
        Vector2[] uvs = new Vector2[vcount];
        int[][] allTriangles = new int[typescnt][];
        for (int i = 0; i < allTriangles.Length; i++)
        {
            allTriangles[i] = new int[cellTypeCounts[i + 1] * celltcnt];           
        }

        int[] idx = new int[typescnt];       

        for (int x = 0; x < World.BlockSize; x++)
        {
            for (int y = 0; y < World.BlockSize; y++)
            {
                int ctype = (int)cellTypes[x * World.BlockSize + y];
                if (ctype == 0) continue;
               
                int it = idx[ctype - 1]++;
                int iv = it + (ctype == 1 ? 0 : cellTypeCounts[ctype - 1]);
                //Debug.Log("Type " + ctype + ", it: " + it + ", iv: " + iv);           
                Vector2 coord = new HexXY(x, y).ToPlaneCoordinates();                
                for (int v = 0; v < cellvcnt; v++)
                {
                    int ivv = iv * cellvcnt + v;
                    float vx = cellVertices[v].x * G.g.hexInset + coord.x;
                    float vy = cellVertices[v].y * G.g.hexInset + coord.y;

                    vertices[ivv].x = vx;
                    vertices[ivv].z = vy;
                    uvs[ivv].x = vx * G.g.terrainTexScale;
                    uvs[ivv].y = vy * G.g.terrainTexScale;
                }

                int[] tri = allTriangles[ctype - 1];                
                for (int t = 0; t < celltcnt; t++)
                {                    
                    tri[it * celltcnt + t] = cellTriangles[t] + iv * cellvcnt;
                }
            }
        }      
     
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = new Vector3(0, 1, 0);
        }

        mesh.subMeshCount = typescnt;
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        for (int i = 0; i < typescnt; i++)
        {
            mesh.SetTriangles(allTriangles[i], i);
        }
        mesh.RecalculateBounds();
    }
}
