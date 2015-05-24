using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class DoorGraphics : EntityGraphics, IClickable, IHighlightable
{
    public VariableBool isOpen;
    public float doorHeight = 1.5f;
    public float topTexScale = 0.5f;
    public float sideTexScale = 0.5f;
    public float inset = 0.8f;

    public AnimationCurve openAnimation;
    public float openSpeed;
    float openProgress;

    public bool CanBeHighlighted
    {
        get
        {
            //return openProgress == (isOpen.value ? 1 : 0);
            return false;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        var mesh = new Mesh();
        var vertices = new List<Vector3>();       
        var triangles = new List<int>();
        var uvs = new List<Vector2>();

        //Top
        for (int i = 0; i < 6; i++)
        {
            vertices.Add(new Vector3(HexTerrain.cellVertices[i].x * inset, doorHeight, HexTerrain.cellVertices[i].y * inset));
            uvs.Add(new Vector2(HexTerrain.cellVertices[i].x * topTexScale * inset, HexTerrain.cellVertices[i].y * topTexScale * inset));
        }

        for (int i = 0; i < 12; i++)
            triangles.Add(HexTerrain.cellTriangles[i]);

        //Sides
        for (int i = 0; i < 6; i++)
        {
            vertices.Add(new Vector3(HexTerrain.cellVertices[i].x * inset, doorHeight, HexTerrain.cellVertices[i].y * inset));
            uvs.Add(new Vector2(i / 6f, doorHeight * sideTexScale));
        }

        for (int i = 0; i < 6; i++)
        {
            vertices.Add(new Vector3(HexTerrain.cellVertices[i].x * inset, 0, HexTerrain.cellVertices[i].y * inset));
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

        //Bottom
        for (int i = 11; i >= 0; i--)
            triangles.Add(HexTerrain.cellTriangles[i] + 12);

        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh = mesh;
        transform.GetChild(0).GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    

    void Start()
    {
        if (!G.IsInUnityEditMode() && LevelEditor.S == null)                 
            isOpen = new VariableBool(() => ((Door)entity).isOpen);

        bool _ = isOpen.IsNew;
        openProgress = isOpen.value ? 1 : 0;
        SetOpenClosePosition();

#if UNITY_EDITOR
        if (LevelEditor.S != null) EditorStart();
#endif
    }

    void SetOpenClosePosition()
    {
        float ypos = -openAnimation.Evaluate(openProgress) * doorHeight * 0.99f;
        transform.GetChild(0).transform.position = new Vector3(transform.GetChild(0).transform.position.x, ypos, transform.GetChild(0).transform.position.z);
    }

    void Update()
    {        
        bool isOpenNew = isOpen.IsNew;
        float prevProgress = openProgress;        

        if (isOpen.value)        
            openProgress = Mathf.Min(1, openProgress + Time.deltaTime / openSpeed);        
        else        
            openProgress = Mathf.Max(0, openProgress - Time.deltaTime / openSpeed);

        if (openProgress != prevProgress)
            SetOpenClosePosition();
        
#if UNITY_EDITOR
        if (LevelEditor.S != null) EditorUpdate(isOpenNew);
#endif
    }

#if UNITY_EDITOR
    public ScriptObjectID id;

    

    public override Entity CreateEntity()
    {
        return new Door() { id = (ScriptObjectID)id.id, isOpen = isOpen.value };
    }

    void EditorStart()
    {
        Door door = (Door)entity;       
        id = (ScriptObjectID)door.id.id;
        isOpen.value = door.isOpen;

        if (!isOpen.value)
            LevelEditor.S.ChangeStaticPassability(door.pos, false);
        SetOpenClosePosition();
    }

    void EditorUpdate(bool isOpenNew)
    {
        Door door = (Door)entity;
        door.id.id = id.id;
        if (isOpenNew)
        {            
            door.isOpen = isOpen.value;            
            LevelEditor.S.ChangeStaticPassability(door.pos, false);
        }
    }
#endif
}

