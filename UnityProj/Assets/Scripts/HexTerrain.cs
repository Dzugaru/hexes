using UnityEngine;
using Engine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexTerrain : MonoBehaviour
{
    static readonly float r3 = Mathf.Sqrt(3);

    GameObject wallPrefab;
    GameObject wallsParent;

    public bool enableWalls = true;

    public static Vector2[] cellVertices = new Vector2[]
        {
            new Vector2(-0.5f / r3, 0.5f),
            new Vector2(0.5f / r3, 0.5f),
            new Vector2(1f / r3, 0),
            new Vector2(0.5f / r3, -0.5f),
            new Vector2(-0.5f / r3, -0.5f),
            new Vector2(-1f / r3, 0),
        };

    public static int[] cellTriangles = new int[]
        {
            0, 1, 2,//Right triangle
            2, 3, 4,//Bottom-left triangle
            4, 5, 0,//Upper-left triangle
            0, 2, 4//Center triangle
        };

    void Awake()
    {
        wallPrefab = Resources.Load<GameObject>("Prefabs/Wall");

        //Generate wall mesh for now, model later    
        //float wallHeight = 1.5f;
        //float topTexScale = 0.5f;
        //float sideTexScale = 0.5f;

        //wallPrefab = new GameObject("Wall");

        //wallPrefab.transform.SetParent(gameObject.transform, false);
        //wallPrefab.SetActive(false);
        //var meshRenderer = wallPrefab.AddComponent<MeshRenderer>();
        //var meshFilter = wallPrefab.AddComponent<MeshFilter>();
        //var mesh = new Mesh();

        //meshRenderer.sharedMaterial = (Material)Resources.Load("Terrain/Wall");

        //var vertices = new List<Vector3>();
        //var uvs = new List<Vector2>();
        //var triangles = new List<int>();

        ////Top
        //for (int i = 0; i < 6; i++)
        //{
        //    vertices.Add(new Vector3(cellVertices[i].x, wallHeight, cellVertices[i].y));
        //    uvs.Add(new Vector2(cellVertices[i].x * topTexScale, cellVertices[i].y * topTexScale));
        //}

        //for (int i = 0; i < 12; i++)
        //    triangles.Add(cellTriangles[i]);

        ////Sides
        //for (int i = 0; i < 6; i++)
        //{
        //    vertices.Add(new Vector3(cellVertices[i].x, wallHeight, cellVertices[i].y));
        //    uvs.Add(new Vector2(i / 6f, wallHeight * sideTexScale));
        //}

        //for (int i = 0; i < 6; i++)
        //{
        //    vertices.Add(new Vector3(cellVertices[i].x, 0, cellVertices[i].y));
        //    uvs.Add(new Vector2(i / 6f, 0));
        //}

        //for (int i = 0; i < 6; i++)
        //{
        //    triangles.Add(6 + i);
        //    triangles.Add(12 + i);
        //    triangles.Add(6 + ((i + 1) % 6));
        //    triangles.Add(6 + ((i + 1) % 6));
        //    triangles.Add(12 + i);
        //    triangles.Add(12 + ((i + 1) % 6));
        //}

        //mesh.vertices = vertices.ToArray();
        //mesh.uv = uvs.ToArray();
        //mesh.triangles = triangles.ToArray();
        //mesh.RecalculateNormals();
        //mesh.RecalculateBounds();

        //meshFilter.mesh = mesh;
    }

    void Update()
    {
        if (enableWalls != wallsParent.activeSelf)        
            wallsParent.SetActive(enableWalls);
    }

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
                    int ctype = (int)wb.GetCellType(new HexXY(x, y));
                    if (ctype != k) continue;

                    //Main tile     
                    HexXY curr = new HexXY(x, y);
                    Vector2 coord = curr.ToPlaneCoordinates();
                    for (int v = 0; v < cellvcnt; v++)
                    {
                        float vx = cellVertices[v].x + coord.x;
                        float vy = cellVertices[v].y + coord.y;

                        vertices.Add(new Vector3(vx, 0, vy));
                        uvs.Add(new Vector2(vx * terrainTexScale, vy * terrainTexScale));
                    }

                    for (int t = 0; t < celltcnt; t++)
                        triangles.Add(vertices.Count - cellvcnt + cellTriangles[t]);

                    //Fill edges                    
                    //for (int n = 0; n < 6; n++)
                    //{
                    //    HexXY ng = curr + HexXY.neighbours[n];
                    //    HexXY ngr = curr + HexXY.neighbours[(n + 1) % 6];
                    //    HexXY ngrn = ngr + HexXY.neighbours[n];
                    //    Vector2 ngCoord = ng.ToPlaneCoordinates();
                    //    if (wb.GetCellType(ng) != TerrainCellType.Empty &&
                    //       wb.GetCellType(ngr) == TerrainCellType.Empty &&
                    //       wb.GetCellType(ngrn) == TerrainCellType.Empty)
                    //    {
                    //        Vector2 vert = cellVertices[(n + 1) % 6] + coord;
                    //        vertices.Add(new Vector3(vert.x, 0, vert.y));
                    //        uvs.Add(new Vector2(vert.x * terrainTexScale, vert.y * terrainTexScale));

                    //        vert = cellVertices[(n + 2) % 6] + coord;
                    //        vertices.Add(new Vector3(vert.x, 0, vert.y));
                    //        uvs.Add(new Vector2(vert.x * terrainTexScale, vert.y * terrainTexScale));

                    //        vert = cellVertices[(n + 2) % 6] + ngCoord;
                    //        vertices.Add(new Vector3(vert.x, 0, vert.y));
                    //        uvs.Add(new Vector2(vert.x * terrainTexScale, vert.y * terrainTexScale));

                    //        triangles.Add(vertices.Count - 3 + 1);
                    //        triangles.Add(vertices.Count - 3);
                    //        triangles.Add(vertices.Count - 3 + 2);
                    //    }
                    //}
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
                    var currType = wb.GetCellType(new HexXY(x, y));
                    for (int n = 0; n < 3; n++)
                    {
                        HexXY curr = new HexXY(x, y);
                        HexXY ng = curr + HexXY.neighbours[n];
                        HexXY ngr = curr + HexXY.neighbours[(n + 1) % 6];
                        if (currType == TerrainCellType.Empty &&
                            (!WorldBlock.IsInRange(ng) || wb.GetCellType(ng) == TerrainCellType.Empty)) continue;

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
                        if (currType == TerrainCellType.Empty &&
                            (!WorldBlock.IsInRange(ngr) || wb.GetCellType(ngr) == TerrainCellType.Empty) &&
                            (!WorldBlock.IsInRange(ng) || wb.GetCellType(ng) == TerrainCellType.Empty)) continue;
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

        GenerateWalls(wb);
    }

    public void GenerateWalls(WorldBlock wb)
    {
        wallsParent = new GameObject("Walls");
        wallsParent.SetActive(enableWalls);
        wallsParent.transform.SetParent(transform, false);
        wallsParent.transform.localPosition = Vector3.zero;    

        for (int x = -1; x <= WorldBlock.sz; x++)
        {
            for (int y = -1; y <= WorldBlock.sz; y++)
            {                
                HexXY p = new HexXY(x, y);
                Vector2 coords = p.ToPlaneCoordinates();
                //Need to request other blocks in case we're drawing outside wall
                if ((!WorldBlock.IsInRange(p) && Level.S.GetCellType(p + new HexXY(wb.position.x * WorldBlock.sz, wb.position.y * WorldBlock.sz)) != TerrainCellType.Empty) ||
                     WorldBlock.IsInRange(p) && wb.GetCellType(p) != TerrainCellType.Empty) continue;

                bool hasNeigh = false;
                for (int n = 0; n < 6; n++)
                {
                    HexXY np = p + HexXY.neighbours[n];                    
                    if (!WorldBlock.IsInRange(np)) continue;

                    if (wb.GetCellType(np) != TerrainCellType.Empty)
                    {
                        hasNeigh = true;
                        break;
                    }
                }

                if (!hasNeigh) continue;

                var wall = Instantiate(wallPrefab);
                wall.transform.SetParent(wallsParent.transform, false);
                wall.transform.localPosition = new Vector3(coords.x, 0, coords.y);
                wall.SetActive(true);
            }
        }                
    }

    public void SaveMeshesToAssets(string id)
    {
#if UNITY_EDITOR
        foreach (Transform t in transform)
        {
            var f = t.GetComponent<MeshFilter>();
            if (f == null) continue;

            f.mesh.Optimize();

            string folder = "Assets/MyAssets/PersistTerrain/" + Application.loadedLevelName + "/" + id + "/";
            if (!System.IO.Directory.Exists(folder))            
                System.IO.Directory.CreateDirectory(folder);            
            string path = folder + t.gameObject.name + ".asset";
            UnityEditor.AssetDatabase.DeleteAsset(path);
            UnityEditor.AssetDatabase.CreateAsset(f.mesh, path);
            f.mesh = (Mesh)UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(Mesh));
        }
#endif
    }
}
