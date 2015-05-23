﻿using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[ExecuteInEditMode]
public class PressPlateGraphics : EntityGraphics
{
    MeshRenderer renderer;
    Material origMaterial;

    public float ypos = 0.01f, inset = 1;

    public VariableBool isPressed;
    public Color pressEmissionColor;

    void OnEnable()
    {
        if (G.IsEditing())
            isPressed = new VariableBool() { value = false };
        else
            isPressed = new VariableBool(() => ((PressPlate)entity).isPressed);
    } 

    void Awake()
    {
        var mesh = new Mesh();
        var vertices = new List<Vector3>();       
        var triangles = new List<int>();
        var uvs = new List<Vector2>();

        for (int i = 0; i < 6; i++)
        {
            vertices.Add(new Vector3(HexTerrain.cellVertices[i].x * inset, ypos, HexTerrain.cellVertices[i].y * inset));
            uvs.Add(new Vector2(HexTerrain.cellVertices[i].x * inset, HexTerrain.cellVertices[i].y * inset));
        }

        for (int i = 0; i < 12; i++)
            triangles.Add(HexTerrain.cellTriangles[i]);

        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        transform.GetChild(0).GetComponent<MeshFilter>().mesh = mesh;

        renderer = transform.GetChild(0).GetComponent<MeshRenderer>();
        origMaterial = renderer.sharedMaterial;
    }

    void Update()
    {
        if (isPressed.IsNew)
        {
            if (isPressed.value)
            {   
                renderer.material.SetColor("_EmissionColor", pressEmissionColor);                
            }
            else
            {   
                Destroy(renderer.material);
                renderer.sharedMaterial = origMaterial;
            }
        }
    }
}

