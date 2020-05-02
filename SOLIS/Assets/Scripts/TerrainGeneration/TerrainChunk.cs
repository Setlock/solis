using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.GameCenter;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class TerrainChunk
{
    Transform parent;
    bool loaded = false;
    bool remove = false;
    GameObject myObject;
    Mesh mesh;
    public Vector2 position;
    Vector2 viewPosition;
    public int width, height;
    public float featureSize;
    Texture2D spritemap;
    TilesetLookup tilesetLookup;
    public Dictionary<Vector2, TileData> tileDictionary = new Dictionary<Vector2, TileData>();
    Vector3[] vertices;
    Vector2[] uv;
    Color[] colors;
    Color mainColor;
    SimplexNoiseGenerator noise;
    Material terrainMat;
    public TerrainChunk(Material terrainMat, SimplexNoiseGenerator noise, Transform parent, Vector3[] vertices, Vector2[] uv, Dictionary<Vector2, TileData> tileData, Texture2D spritemap, TilesetLookup tilesetLookup, Vector2 position, Vector2 viewPosition, int width, int height, float featureSize, Color[] colors, Color mainColor)
    {
        this.terrainMat = terrainMat;
        this.noise = noise;
        this.parent = parent;
        this.vertices = vertices;
        this.uv = uv;
        this.tileDictionary = tileData;
        this.spritemap = spritemap;
        this.tilesetLookup = tilesetLookup;
        this.position = position;
        this.viewPosition = viewPosition;
        this.width = width;
        this.height = height;
        this.featureSize = featureSize;
        this.colors = colors;
        this.mainColor = mainColor;
    }
    public void GenerateChunk()
    {
        //Create GameObject with unique name and mesh components
        myObject = new GameObject("Terrain Chunk" + position, typeof(MeshFilter), typeof(MeshRenderer));
        myObject.isStatic = true;
        //myObject.isStatic = true;
        //Set parent for organization in Editor
        myObject.transform.parent = parent;
        //Set positon to be relative to given terrain chunk position
        myObject.transform.position = new Vector3(viewPosition.x, viewPosition.y);

        //Create empty mesh object with unique name
        mesh = new Mesh();
        mesh.name = "Chunk Mesh" + viewPosition;
        //Create list to add triangles
        List<int> trianglesList = new List<int>();

        foreach (Vector2 key in tileDictionary.Keys)
        {
            TileData tileData = tileDictionary[key];
            Vector3 pos = new Vector3((position.x + key.x) / featureSize, (position.y + key.y) / featureSize);
            float val = (float)noise.Evaluate(pos.x, pos.y, pos.z);
            if (GetTileStateFromNoise(val))
            {
                tileData.SetState(true);

                int vi = tileData.GetVertexIndex();
                if (vi != -1)
                {
                    //Add vertex location to draw 2 triangles
                    trianglesList.Add(vi);
                    trianglesList.Add(vi + 2);
                    trianglesList.Add(vi + 1);

                    trianglesList.Add(vi + 1);
                    trianglesList.Add(vi + 2);
                    trianglesList.Add(vi + 3);

                    //Set colors of vertices to planet color
                    colors[vi] = mainColor;
                    colors[vi + 1] = mainColor;
                    colors[vi + 2] = mainColor;
                    colors[vi + 3] = mainColor;
                }
            }
            else
            {
                tileData.SetState(false);
            }
        }

        mesh.vertices = vertices;
        UpdateTileUV();

        myObject.AddComponent<CompositeCollider2D>();
        myObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

        //Set mesh triangles
        mesh.triangles = trianglesList.ToArray();
        mesh.colors = colors;

        //Set material texture to use terrain spritemap
        //Set Mesh material to created material
        myObject.GetComponent<MeshRenderer>().sharedMaterial = terrainMat;
        myObject.GetComponent<MeshRenderer>().sortingOrder = 2;
        //Optimize mesh
        mesh.Optimize();
        //Set GameObject mesh to generated mesh
        myObject.GetComponent<MeshFilter>().sharedMesh = mesh;
        //Set loaded = true to denote chunk being generated
        loaded = true;
    }
    /// <summary>
    /// Returns state of tile based on noise val and same arbitrary number
    /// </summary>
    public bool GetTileStateFromNoise(float val)
    {
        if(val >= -0.4f)
        {
            return true;
        }
        return false;
    }
    /// <summary>
    /// Set chunk loaded = false
    /// Disable chunk GameObject
    /// </summary>
    public void Remove()
    {
        loaded = false;
        myObject.SetActive(false);
        tileDictionary.Clear();
    }
    /// <summary>
    /// Return data of tile at specific position
    /// </summary>
    public TileData GetTileData(Vector2 position)
    {
        TileData tileData = null;
        if (tileDictionary.ContainsKey(position))
        {
            tileData = tileDictionary[position];
        }
        return tileData;
    }
    /// <summary>
    /// Set all tile uvs based on surrounding tiles
    /// </summary>
    public void UpdateTileUV()
    {
        float tileWidth = tilesetLookup.getTileWidth() / (float)spritemap.width;
        float tileHeight = tilesetLookup.getTileHeight() / (float)spritemap.height;
        foreach (Vector2 key in tileDictionary.Keys)
        {
            TileData tileData = tileDictionary[key];
            if (tileData.GetVertexIndex() != -1)
            {
                TileData tileTL = tileData.adjTiles[0];
                TileData tileT = tileData.adjTiles[1];
                TileData tileTR = tileData.adjTiles[2];

                TileData tileL = tileData.adjTiles[3];
                TileData tileR = tileData.adjTiles[4];

                TileData tileBL = tileData.adjTiles[5];
                TileData tileB = tileData.adjTiles[6];
                TileData tileBR = tileData.adjTiles[7];

                //Vector2 uvCoord = tilesetLookup.GetPosition(tilesetLookup.GetName(binaryState));
                int vi = tileData.GetVertexIndex();
                string tileName = "Grass";
                //NOTE: ALL VALUES ARE SELECTED BASED ON GIVEN SPRITEMAP
                //THIS WILL CHANGE IN THE FUTURE TO BE AUTOMATICALLY CREATED RATHER THAN HARDCODED
                if (tileT != null && !tileT.GetState())
                {
                    tileName = "TopEdge";
                    if (tileR != null && !tileR.GetState())
                    {
                        tileName = "CornerTR";
                    }
                    else if (tileL != null && !tileL.GetState())
                    {
                        tileName = "CornerTL";
                    }
                }
                else if (tileB != null && !tileB.GetState())
                {
                    tileName = "BottomEdge";
                    if (tileR != null && !tileR.GetState())
                    {
                        tileName = "CornerBR";
                    }
                    else if (tileL != null && !tileL.GetState())
                    {
                        tileName = "CornerBL";
                    }
                }
                else if (tileR != null && !tileR.GetState())
                {
                    tileName = "RightEdge";
                }
                else if (tileL != null && !tileL.GetState())
                {
                    tileName = "LeftEdge";
                }
                else if (tileBR != null && !tileBR.GetState())
                {
                    tileName = "CurveTL";
                }
                else if (tileTR != null && !tileTR.GetState())
                {
                    tileName = "CurveBL";
                }
                else if (tileBL != null && !tileBL.GetState())
                {
                    tileName = "CurveTR";
                }
                else if (tileTL != null && !tileTL.GetState())
                {
                    tileName = "CurveBR";
                }
                if (tileData.GetState() && !tileName.Equals("Grass"))
                {
                    BoxCollider2D box = myObject.AddComponent<BoxCollider2D>();
                    box.offset = new Vector2(vertices[vi].x+0.5f,vertices[vi].y+0.5f);
                    box.usedByComposite = true;
                }
                Vector2 uvCoord = tilesetLookup.GetPosition(tileName);
                float textureOffsetX = uvCoord.x / (float)spritemap.width;
                float textureOffsetY = uvCoord.y / (float)spritemap.height;

                uv[vi] = new Vector2(textureOffsetX, textureOffsetY);
                uv[vi + 1] = new Vector2(textureOffsetX + tileWidth, textureOffsetY);
                uv[vi + 2] = new Vector2(textureOffsetX, textureOffsetY + tileHeight);
                uv[vi + 3] = new Vector2(textureOffsetX + tileWidth, textureOffsetY + tileHeight);
            }
        }

        mesh.uv = uv;
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
public class TileData
{
    public TileData[] adjTiles = new TileData[8];
    int vertexIndex;
    bool state = false;
    int binaryState = 0;
    public TileData(int vi, bool state)
    {
        this.vertexIndex = vi;
        this.state = state;
        if (state)
        {
            binaryState = 1;
        }
    }
    public void SetVertexIndex(int vi)
    {
        this.vertexIndex = vi;
    }
    public int GetVertexIndex()
    {
        return vertexIndex;
    }
    public int GetBinaryState()
    {
        return binaryState;
    }
    public void SetState(bool state)
    {
        this.state = state;
    }
    public bool GetState()
    {
        return state;
    }
}
