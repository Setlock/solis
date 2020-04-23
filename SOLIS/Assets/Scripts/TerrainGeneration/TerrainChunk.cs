using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

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
    Texture2D spritemap;
    Vector3[] vertices;
    Vector2[] uv;
    public TerrainChunk(Transform parent, Vector3[] vertices, Vector2[] uv, Texture2D spritemap, Vector2 position, int width, int height, float featureSize)
    {
        this.parent = parent;
        this.vertices = vertices;
        this.uv = uv;
        this.spritemap = spritemap;
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
        //Create GameObject with unique name and mesh components
        myObject = new GameObject("Terrain Chunk" + position, typeof(MeshFilter), typeof(MeshRenderer));
        //Set parent for organization in Editor
        myObject.transform.parent = parent;
        //Set positon to be relative to given terrain chunk position
        myObject.transform.position = new Vector3(position.x, position.y);

        //Create empty mesh object with unique name
        mesh = new Mesh();
        mesh.name = "Chunk Mesh" + position;

        //Create empty array of for index positions
        int[] triangles = new int[width*height*6];

        //Create variables to calculate proper index
        int vi = 0;
        int ti = 0;
        int xPos = 0;
        int yPos = 0;
        //Loop through all vertices adding vertex to triangles array at correct location
        while (vi < vertices.Length)
        {
            //Get current tile position
            Vector3 pos = new Vector3((position.x + xPos) / featureSize, (position.y + yPos) / featureSize);
            //Get value from Simplex Noise
            float val = Noise.Simplex2D(pos, 2).value;
            //Check if value is above some arbitrary number if it is then draw that quad
            if (val >= -.4f)
            {
                //Add vertex location to draw 2 triangles
                triangles[ti] = vi;
                triangles[ti + 1] = vi + 2;
                triangles[ti + 2] = vi + 1;

                triangles[ti + 3] = vi + 1;
                triangles[ti + 4] = vi + 2;
                triangles[ti + 5] = vi + 3;
            }
            //Increment triangle index and vertex index
            ti += 6;
            vi += 4;

            //Calculate x and y position for proper noise calculation
            xPos++;
            if (xPos >= width)
            {
                yPos++;
                xPos = 0;
            }
        }

        //Set mesh vertices, uvs and triangles
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        //Set material of mesh to a new material using default sprite shader
        Material mat = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default"));
        //Set material texture to use terrain spritemap
        mat.mainTexture = spritemap;
        //Set Mesh material to created material
        myObject.GetComponent<MeshRenderer>().sharedMaterial = mat;
        //Optimize mesh
        mesh.Optimize();
        ///Set GameObject mesh to generated mesh
        myObject.GetComponent<MeshFilter>().sharedMesh = mesh;

        //Set loaded = true to denote chunk being generated
        loaded = true;
    }
    public Vector2 ConvertPixelsToUVCoord(int x, int y, int textureWidth, int textureHeight)
    {
        return new Vector2((float)x / textureWidth, (float)y / textureHeight);
    }
    /// <summary>
    /// Set chunk loaded = false
    /// Disable chunk GameObject
    /// </summary>
    public void Remove()
    {
        loaded = false;
        myObject.SetActive(false);
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
