using UnityEngine;
using Engine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexTerrain : MonoBehaviour
{
    static readonly float r3 = Mathf.Sqrt(3);

    Mesh mesh;

    static Vector2[] cellVertices = new Vector2[]
        {                       
            new Vector2(-0.5f / r3, 0.5f),
            new Vector2(0.5f / r3, 0.5f),
            new Vector2(1f / r3, 0),
            new Vector2(0.5f / r3, -0.5f),
            new Vector2(-0.5f / r3, -0.5f),
            new Vector2(-1f / r3, 0),
        };

    static int[] cellTriangles = new int[]
        {
            0, 1, 2,//Right triangle
            2, 3, 4,//Bottom-left triangle
            4, 5, 0,//Upper-left triangle
            0, 2, 4//Center triangle
        };   

    public void Generate(WorldBlock wb, float hexInset, float terrainTexScale)
    {
        var cellTypes = wb.cellTypes;
        var cellTypeCounts = wb.cellTypeCounts;
        var nonEmptyCellsCount = wb.nonEmptyCellsCount;

        mesh = new Mesh();
        mesh.name = "Hex Terrain mesh";
        GetComponent<MeshFilter>().mesh = mesh;

        int typescnt = WorldBlock.terrainTypesCount - 1;
        var mats = new Material[typescnt];
        mats[0] = (Material)Resources.Load("Terrain/DryGround");
        mats[1] = (Material)Resources.Load("Terrain/Grass");
        mats[2] = (Material)Resources.Load("Terrain/Snow");
        GetComponent<MeshRenderer>().sharedMaterials = mats;

        
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

        for (int x = 0; x < WorldBlock.sz; x++)
        {
            for (int y = 0; y < WorldBlock.sz; y++)
            {
                int ctype = (int)cellTypes[x,y];
                if (ctype == 0) continue;

                int it = idx[ctype - 1]++;
                int iv = it + (ctype == 1 ? 0 : cellTypeCounts[ctype - 1]);
                //Debug.Log("Type " + ctype + ", it: " + it + ", iv: " + iv);           
                Vector2 coord = new HexXY(x, y).ToPlaneCoordinates();
                for (int v = 0; v < cellvcnt; v++)
                {
                    int ivv = iv * cellvcnt + v;
                    float vx = cellVertices[v].x * hexInset + coord.x;
                    float vy = cellVertices[v].y * hexInset + coord.y;

                    vertices[ivv].x = vx;
                    vertices[ivv].z = vy;
                    uvs[ivv].x = vx * terrainTexScale;
                    uvs[ivv].y = vy * terrainTexScale;
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

    //void GenerateWalls()
    //{
    //    Mesh mesh = new Mesh();
    //    mesh.name = "Walls";
    //    GameObject walls = transform.GetChild(0).gameObject;
    //    walls.GetComponent<MeshFilter>().mesh = mesh;
    //}

    public void GenerateMultiple(WorldBlock wb, float hexInset, float terrainTexScale)
    {
        int cellvcnt = cellVertices.Length;
        int celltcnt = cellTriangles.Length;
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        var triangles = new List<int>();

        for (int k = 1; k < WorldBlock.terrainTypesCount; k++)
        {
            if (wb.cellTypeCounts[k] == 0) continue;

            string terrainTypeName = ((TerrainCellType)k).ToString();
            GameObject obj = new GameObject(terrainTypeName);
            obj.transform.SetParent(transform, false);
            var meshRenderer = obj.AddComponent<MeshRenderer>();
            var meshFilter = obj.AddComponent<MeshFilter>();
            var mesh = new Mesh();

            meshRenderer.sharedMaterial = (Material)Resources.Load("Terrain/" + terrainTypeName);         

            for (int x = 0; x < WorldBlock.sz; x++)
            {
                for (int y = 0; y < WorldBlock.sz; y++)
                {
                    int ctype = (int)wb.cellTypes[x, y];
                    if (ctype != k) continue;                 
                    
                    //Main tile     
                    Vector2 coord = new HexXY(x, y).ToPlaneCoordinates();
                    for (int v = 0; v < cellvcnt; v++)
                    {                        
                        float vx = cellVertices[v].x + coord.x;
                        float vy = cellVertices[v].y + coord.y;

                        vertices.Add(new Vector3(vx, 0, vy));
                        uvs.Add(new Vector2(vx * terrainTexScale, vy * terrainTexScale));
                    }

                    for (int t = 0; t < celltcnt; t++)
                        triangles.Add(vertices.Count - cellvcnt + cellTriangles[t]);                    
                }
            }

            for (int i = 0; i < vertices.Count; i++)            
                normals.Add(new Vector3(0, 1, 0));

            mesh.vertices = vertices.ToArray();
            mesh.normals = normals.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.triangles = triangles.ToArray();            
            mesh.RecalculateBounds();

            meshFilter.mesh = mesh;

            vertices.Clear();
            normals.Clear();
            uvs.Clear();
            triangles.Clear();
        }

        //Grid
        {
            float gridYPos = 0.02f;
            GameObject obj = new GameObject("Grid");
            obj.transform.SetParent(transform, false);
            var meshRenderer = obj.AddComponent<MeshRenderer>();
            var meshFilter = obj.AddComponent<MeshFilter>();
            var mesh = new Mesh();

            meshRenderer.sharedMaterial = (Material)Resources.Load("Terrain/Grid");

            for (int x = 0; x < WorldBlock.sz; x++)
            {
                for (int y = 0; y < WorldBlock.sz; y++)
                {
                    var currType = wb.cellTypes[x, y];
                    if (currType == TerrainCellType.Empty) continue;                  
                    for (int n = 0; n < 3; n++)
                    {
                        HexXY curr = new HexXY(x, y);
                        HexXY ng = curr + HexXY.neighbours[n];
                        HexXY ngr = curr + HexXY.neighbours[n + 1];
                        if (WorldBlock.S.GetCellType(ng) == TerrainCellType.Empty) continue;

                        Vector2 currCoord = curr.ToPlaneCoordinates();
                        Vector2 ngCoord = ng.ToPlaneCoordinates();

                        //Line between two
                        Vector2 vert = cellVertices[n] * hexInset + currCoord;
                        vertices.Add(new Vector3(vert.x, gridYPos, vert.y));

                        vert = cellVertices[(n + 1) % 6] * hexInset + currCoord;
                        vertices.Add(new Vector3(vert.x, gridYPos, vert.y));

                        vert = cellVertices[(n + 3) % 6] * hexInset + ngCoord;
                        vertices.Add(new Vector3(vert.x, gridYPos, vert.y));

                        vert = cellVertices[(n + 4) % 6] * hexInset + ngCoord;
                        vertices.Add(new Vector3(vert.x, gridYPos, vert.y));

                        triangles.Add(vertices.Count - 4 + 1);
                        triangles.Add(vertices.Count - 4);                        
                        triangles.Add(vertices.Count - 4 + 2);
                        triangles.Add(vertices.Count - 4 + 2);
                        triangles.Add(vertices.Count - 4);                        
                        triangles.Add(vertices.Count - 4 + 3);

                        //Triangle between three
                        if (WorldBlock.S.GetCellType(ngr) == TerrainCellType.Empty) continue;
                        Vector2 ngrCoord = ngr.ToPlaneCoordinates();

                        vert = cellVertices[(n + 1) % 6] * hexInset + currCoord;
                        vertices.Add(new Vector3(vert.x, gridYPos, vert.y));

                        vert = cellVertices[(n + 3) % 6] * hexInset + ngCoord;
                        vertices.Add(new Vector3(vert.x, gridYPos, vert.y));

                        vert = cellVertices[(n + 5) % 6] * hexInset + ngrCoord;
                        vertices.Add(new Vector3(vert.x, gridYPos, vert.y));
                        
                        triangles.Add(vertices.Count - 3);
                        triangles.Add(vertices.Count - 3 + 1);
                        triangles.Add(vertices.Count - 3 + 2);
                    }
                }
            }

          

            for (int i = 0; i < vertices.Count; i++)
                normals.Add(new Vector3(0, 1, 0));

            mesh.vertices = vertices.ToArray();
            mesh.normals = normals.ToArray();            
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateBounds();

            meshFilter.mesh = mesh;
        }
    }
}
