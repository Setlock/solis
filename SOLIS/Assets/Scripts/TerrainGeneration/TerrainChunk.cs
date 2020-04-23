using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Mathematics;
using UnityEditor.SceneManagement;
using UnityEngine;

public class TerrainChunk
{
    Transform parent;
    bool loaded = false;
    bool remove = false;
    GameObject myObject;
    Mesh mesh;
    Vector2 position;
    int width, height;
    float featureSize;
    public TerrainChunk(Transform parent, Vector2 position, int width, int height, float featureSize)
    {
        this.parent = parent;
        this.position = position;
        this.width = width;
        this.height = height;
        this.featureSize = featureSize;
        GenerateMesh();
    }
    /// <summary>
    /// Generate new GameObject for terrain chunk that contains mesh of tiles
    /// </summary>
    public void GenerateMesh()
    {
        //Create GameObject with unique name
        myObject = new GameObject("Terrain Chunk" + position);
        //Set parent for organization in Editor
        myObject.transform.parent = parent;
        //Set positon to be relative to given terrain chunk position
        myObject.transform.position = new Vector3(position.x, position.y);

        //Create empty mesh object with unique name
        mesh = new Mesh();
        mesh.name = "Chunk Mesh" + position;

        //Create empty array of for vertex positions and index positions
        Vector3[] vertices = new Vector3[(width+1)*(height+1)];
        int[] triangles = new int[width*height*6];

        //Create all vertex positions (generated from bottom left corner of chunk to the top right)
        for(int i = 0, y = 0; y <= height; y++)
        {
            for(int x = 0; x <= width; x++, i++)
            {
                vertices[i] = new Vector3(x, y);
            }
        }
        //Set mesh vertices
        mesh.vertices = vertices;

        //Calculate index position to create quads of mesh
        for (int ti = 0, vi = 0, y = 0; y < height; y++,vi++)
        {
            for(int x = 0; x < width; x++, ti += 6, vi++)
            {
                //Get a noise value between 1 and -1 based on current x position
                Vector3 pos = new Vector3((position.x + x) / featureSize, (position.y + y) / featureSize);
                float val = Noise.Simplex2D(pos, 2).value;
                //Only place tile if value is above some arbitrary threshold
                if (val >= -.4f)
                {
                    //Set index positions
                    triangles[ti] = vi;
                    triangles[ti + 1] = vi + width + 1;
                    triangles[ti + 2] = vi + 1;

                    triangles[ti + 3] = vi + 1;
                    triangles[ti + 4] = vi + width + 1;
                    triangles[ti + 5] = vi + width + 2;
                }
            }
        }
        //Set mesh triangles
        mesh.triangles = triangles;

        //Set maaterial of mesh to a new material using basic shader
        myObject.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
        //Add MeshFilter
        myObject.AddComponent<MeshFilter>();
        ///Set GameObject mesh to generated mesh
        myObject.GetComponent<MeshFilter>().sharedMesh = mesh;

        //Set loaded = true to denote chunk being generated
        loaded = true;
    }
    /// <summary>
    /// Set chunk loaded = false
    /// </summary>
    public void Remove()
    {
        loaded = false;
    }
    public GameObject GetGameObject()
    {
        return myObject;
    }
    public Mesh GetMesh()
    {
        return this.mesh;
    }
    public void SetRemove(bool shouldRemove)
    {
        this.remove = shouldRemove;
    }
    public bool ShouldRemove()
    {
        return remove;
    }
    public bool IsLoaded()
    {
        return loaded;
    }
}
