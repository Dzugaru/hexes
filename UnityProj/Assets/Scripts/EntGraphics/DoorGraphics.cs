using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[ExecuteInEditMode]
public class DoorGraphics : EntityGraphics
{
    public VariableBool isOpen;
    public float doorHeight = 1.5f;
    public float topTexScale = 0.5f;
    public float sideTexScale = 0.5f;

    void OnEnable()
    {
        if (G.IsEditing())
            isOpen = new VariableBool() { value = false };
        else
            isOpen = new VariableBool(() => ((Door)entity).isOpen);
    } 

    void Awake()
    {
        var mesh = new Mesh();
        var vertices = new List<Vector3>();       
        var triangles = new List<int>();
        var uvs = new List<Vector2>();

        //Top
        for (int i = 0; i < 6; i++)
        {
            vertices.Add(new Vector3(HexTerrain.cellVertices[i].x, doorHeight, HexTerrain.cellVertices[i].y));
            uvs.Add(new Vector2(HexTerrain.cellVertices[i].x * topTexScale, HexTerrain.cellVertices[i].y * topTexScale));
        }

        for (int i = 0; i < 12; i++)
            triangles.Add(HexTerrain.cellTriangles[i]);

        //Sides
        for (int i = 0; i < 6; i++)
        {
            vertices.Add(new Vector3(HexTerrain.cellVertices[i].x, doorHeight, HexTerrain.cellVertices[i].y));
            uvs.Add(new Vector2(i / 6f, doorHeight * sideTexScale));
        }

        for (int i = 0; i < 6; i++)
        {
            vertices.Add(new Vector3(HexTerrain.cellVertices[i].x, 0, HexTerrain.cellVertices[i].y));
            uvs.Add(new Vector2(i / 6f, 0));
        }

        for (int i = 0; i < 6; i++)
        {
            triangles.Add(6 + i);
            triangles.Add(12 + i);
            triangles.Add(6 + ((i + 1) % 6));
            triangles.Add(6 + ((i + 1) % 6));
            triangles.Add(12 + i);
            triangles.Add(12 + ((i + 1) % 6));
        }

        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        transform.GetChild(0).GetComponent<MeshFilter>().mesh = mesh;
    }

    void Update()
    {
        if (isOpen.IsNew)
        {
            if (isOpen.value)
            {   
                
            }
            else
            {   
                
            }
        }
    }
}

