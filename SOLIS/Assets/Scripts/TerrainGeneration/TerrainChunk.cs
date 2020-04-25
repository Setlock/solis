using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Tilemaps;
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
    TilesetLookup tilesetLookup;
    public Dictionary<Vector2, TileData> tileDictionary = new Dictionary<Vector2, TileData>();
    Vector3[] vertices;
    Vector2[] uv;
    Color[] colors;
    public TerrainChunk(Transform parent, Vector3[] vertices, Vector2[] uv, Texture2D spritemap, TilesetLookup tilesetLookup, Vector2 position, int width, int height, float featureSize, Color[] colors)
    {
        this.parent = parent;
        this.vertices = vertices;
        this.uv = uv;
        this.spritemap = spritemap;
        this.tilesetLookup = tilesetLookup;
        this.position = position;
        this.width = width;
        this.height = height;
        this.featureSize = featureSize;
        this.colors = colors;
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
        //Create list to add triangles
        List<int> trianglesList = new List<int>();

        //Create variables to calculate proper index
        int vi = 0;
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
            bool tileState = false;
            if (GetTileStateFromNoise(val))
            {
                //Add vertex location to draw 2 triangles
                trianglesList.Add(vi);
                trianglesList.Add(vi+2);
                trianglesList.Add(vi+1);

                trianglesList.Add(vi+1);
                trianglesList.Add(vi+2);
                trianglesList.Add(vi+3);
                tileState = true;
            }
            //Add tile data for position
            tileDictionary.Add(new Vector2(xPos, yPos), new TileData(vi, tileState));

            //Increment vertex index to next initial quad vertex
            vi += 4;

            //Calculate x and y position for proper noise calculation
            xPos++;
            if (xPos >= width)
            {
                yPos++;
                xPos = 0;
            }
        }
        //Generate tile data for tiles that surround chunk
        GenerateSurroundingTileData();

        //Set the adjacent tile data for all tiles
        SetTileAdjacents();

        //Set mesh vertices and update UVs for all tiles
        mesh.vertices = vertices;
        UpdateTileUV();

        //Set mesh triangles
        mesh.triangles = trianglesList.ToArray();

        //Set material of mesh to a new material using default sprite shader
        Material mat = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default"));
        //Set material texture to use terrain spritemap
        mat.mainTexture = spritemap;
        mesh.colors = colors;
        //Set Mesh material to created material
        myObject.GetComponent<MeshRenderer>().sharedMaterial = mat;
        //Optimize mesh
        mesh.Optimize();
        ///Set GameObject mesh to generated mesh
        myObject.GetComponent<MeshFilter>().sharedMesh = mesh;

        //Set loaded = true to denote chunk being generated
        loaded = true;
    }
    /// <summary>
    /// Generates tile data for tiles that surround chunk 
    /// </summary>
    public void GenerateSurroundingTileData()
    {
        for (int i = -1; i <= width; i++)
        {
            //Calculate noise position at x for top and bottom row
            Vector3 posTopRow = new Vector3((position.x + i) / featureSize, (position.y + -1) / featureSize);
            Vector3 posBottomRow = new Vector3((position.x + i) / featureSize, (position.y + height) / featureSize);

            //Calculate noise val at both positions
            float valTopRow = Noise.Simplex2D(posTopRow, 2).value;
            float valBottomRow = Noise.Simplex2D(posBottomRow, 2).value;

            bool stateTopRow = false, stateBottomRow = false;

            //Get tile state for both positions
            if (GetTileStateFromNoise(valTopRow))
            {
                stateTopRow = true;
            }
            if (GetTileStateFromNoise(valBottomRow))
            {
                stateBottomRow = true;
            }
            //Add tiles to dictionary with vertex index of -1 to avoid adding unecessary vertices
            tileDictionary.Add(new Vector2(i, -1), new TileData(-1, stateTopRow));
            tileDictionary.Add(new Vector2(i, height), new TileData(-1, stateBottomRow));
        }
        //Start at y = 0 and go to y = height-1 to avoide duplicate entries in Dictionary
        for (int j = 0; j < height; j++)
        {
            //Calculate noise position at y for left and right column
            Vector3 posLeftCol = new Vector3((position.x + -1) / featureSize, (position.y + j) / featureSize);
            Vector3 posRightCol = new Vector3((position.x + width) / featureSize, (position.y + j) / featureSize);

            //Calculate noise val at both positions
            float valLeftCol = Noise.Simplex2D(posLeftCol, 2).value;
            float valRightCol = Noise.Simplex2D(posRightCol, 2).value;

            bool stateLeftCol = false, stateRightCol = false;

            //Get tile state for both positions
            if (GetTileStateFromNoise(valLeftCol))
            {
                stateLeftCol = true;
            }
            if (GetTileStateFromNoise(valRightCol))
            {
                stateRightCol = true;
            }
            //Add tiles to dictionary with vertex index of -1 to avoid adding unecessary vertices
            tileDictionary.Add(new Vector2(-1, j), new TileData(-1, stateLeftCol));
            tileDictionary.Add(new Vector2(width, j), new TileData(-1, stateRightCol));
        }
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
    /// Set all adjacent tiles for each tile disregarding tiles created around edge
    /// </summary>
    public void SetTileAdjacents()
    {
        for(int x = 0; x < width; x++)
        {
            for(int y= 0; y < height; y++)
            {
                Vector2 key = new Vector2(x, y);
                TileData tile = tileDictionary[key];
                TileData tileT = GetTileData(new Vector2(key.x, key.y + 1));
                TileData tileTR = GetTileData(new Vector2(key.x + 1, key.y + 1));
                TileData tileTL = GetTileData(new Vector2(key.x - 1, key.y + 1));

                TileData tileB = GetTileData(new Vector2(key.x, key.y - 1));
                TileData tileBR = GetTileData(new Vector2(key.x + 1, key.y - 1));
                TileData tileBL = GetTileData(new Vector2(key.x - 1, key.y - 1));

                TileData tileL = GetTileData(new Vector2(key.x - 1, key.y));
                TileData tileR = GetTileData(new Vector2(key.x + 1, key.y));

                tile.adjTiles[0] = tileTL;
                tile.adjTiles[1] = tileT;
                tile.adjTiles[2] = tileTR;

                tile.adjTiles[3] = tileL;
                tile.adjTiles[4] = tileR;

                tile.adjTiles[5] = tileBL;
                tile.adjTiles[6] = tileB;
                tile.adjTiles[7] = tileBR;
            }
        }
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
        float tileWidth = 64 / (float)spritemap.width;
        float tileHeight = 64 / (float)spritemap.height;
        foreach (TileData tileData in tileDictionary.Values)
        {
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
                Vector2 uvCoord = new Vector2(64f, 64f);

                //NOTE: ALL VALUES ARE SELECTED BASED ON GIVEN SPRITEMAP
                //THIS WILL CHANGE IN THE FUTURE TO BE AUTOMATICALLY CREATED RATHER THAN HARDCODED
                if (tileT != null && !tileT.GetState())
                {
                    uvCoord.x = 64;
                    uvCoord.y = 128;
                    if (tileR != null && !tileR.GetState())
                    {
                        uvCoord.x = 128;
                        uvCoord.y = 128;
                    }
                    else if (tileL != null && !tileL.GetState())
                    {
                        uvCoord.x = 0;
                        uvCoord.y = 128;
                    }
                }
                else if (tileB != null && !tileB.GetState())
                {
                    uvCoord.x = 64;
                    uvCoord.y = 0;
                    if (tileR != null && !tileR.GetState())
                    {
                        uvCoord.x = 128;
                        uvCoord.y = 0;
                    }
                    else if (tileL != null && !tileL.GetState())
                    {
                        uvCoord.x = 0;
                        uvCoord.y = 0;
                    }
                }
                else if (tileR != null && !tileR.GetState())
                {
                    uvCoord.x = 128;
                    uvCoord.y = 64;
                }
                else if (tileL != null && !tileL.GetState())
                {
                    uvCoord.x = 0;
                    uvCoord.y = 64;
                }
                else if (tileBR != null && !tileBR.GetState())
                {
                    uvCoord.x = 192;
                    uvCoord.y = 128;
                }
                else if (tileTR != null && !tileTR.GetState())
                {
                    uvCoord.x = 192;
                    uvCoord.y = 64;
                }
                else if (tileBL != null && !tileBL.GetState())
                {
                    uvCoord.x = 256;
                    uvCoord.y = 128;
                }
                else if (tileTL != null && !tileTL.GetState())
                {
                    uvCoord.x = 256;
                    uvCoord.y = 64;
                }
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
    public int GetVertexIndex()
    {
        return vertexIndex;
    }
    public int GetBinaryState()
    {
        return binaryState;
    }
    public bool GetState()
    {
        return state;
    }
}
