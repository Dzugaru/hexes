#if UNITY_EDITOR
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class StaticBlockedTileVisual : MonoBehaviour
{
    public float ypos;
    public HexXY p;

    void Awake()
    {
        var mesh = new Mesh();
        var vertices = new List<Vector3>();       
        var triangles = new List<int>();
        
        for (int i = 0; i < 6; i++)        
            vertices.Add(new Vector3(HexTerrain.cellVertices[i].x, ypos, HexTerrain.cellVertices[i].y));

        for (int i = 0; i < 12; i++)
            triangles.Add(HexTerrain.cellTriangles[i]);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();


        GetComponent<MeshFilter>().mesh = mesh;
    }
}
#endif

