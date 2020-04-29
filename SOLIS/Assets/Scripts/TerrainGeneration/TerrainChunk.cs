using System;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk
{
    Transform parent;
    bool loaded = false;
    bool remove = false;
    GameObject myObject;
    Mesh mesh;
    Vector2 position, viewPosition;
    int width, height;
    float featureSize;
    Texture2D spritemap;
    TilesetLookup tilesetLookup;
    public Dictionary<Vector2, TileData> tileDictionary = new Dictionary<Vector2, TileData>();
    Vector3[] vertices;
    Vector2[] uv;
    Color[] colors;
    Color mainColor;
    SimplexNoiseGenerator noise;

    public TerrainChunk(SimplexNoiseGenerator noise, Transform parent, Vector3[] vertices, Vector2[] uv, Texture2D spritemap, TilesetLookup tilesetLookup, Vector2 position, Vector2 viewPosition, int width, int height, float featureSize, Color[] colors, Color mainColor)
    {
        this.noise = noise;
        this.parent = parent;
        this.vertices = vertices;
        this.uv = uv;
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
        myObject.transform.position = new Vector3(viewPosition.x, viewPosition.y);

        //Create empty mesh object with unique name
        mesh = new Mesh();
        mesh.name = "Chunk Mesh" + viewPosition;
        //Create list to add triangles
        List<int> trianglesList = new List<int>();

        //Create variables to calculate proper index
        int vi = 0;
        int xPos = 0;
        int yPos = 0;
        //Loop through all vertices adding vertex to triangles array at correct location
        System.Random r = new System.Random();
        while (vi < vertices.Length)
        {
            //Get current tile position
            Vector3 pos = new Vector3((position.x + xPos) / featureSize, (position.y + yPos) / featureSize);
            //Get value from Simplex Noise
            float val = (float)noise.noise(pos.x, pos.y, pos.z);
            //Check if value is above some arbitrary number if it is then draw that quad
            bool tileState = false;
            if (GetTileStateFromNoise(val))
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
        mesh.colors = colors;

        //Set material of mesh to a new material using default sprite shader
        Material mat = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default"));
        //Set material texture to use terrain spritemap
        mat.mainTexture = spritemap;
        //Set Mesh material to created material
        myObject.GetComponent<MeshRenderer>().sharedMaterial = mat;
        myObject.GetComponent<MeshRenderer>().sortingOrder = 2;
        //Optimize mesh
        mesh.Optimize();
        //Set GameObject mesh to generated mesh
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
            float valTopRow = (float)noise.noise(posTopRow.x, posTopRow.y, posTopRow.z);
            float valBottomRow = (float)noise.noise(posBottomRow.x, posBottomRow.y, posBottomRow.z);

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
            float valLeftCol = (float)noise.noise(posLeftCol.x, posLeftCol.y, posLeftCol.z);
            float valRightCol = (float)noise.noise(posRightCol.x, posRightCol.y, posRightCol.z);

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
        float tileWidth = tilesetLookup.getTileWidth() / (float)spritemap.width;
        float tileHeight = tilesetLookup.getTileHeight() / (float)spritemap.height;
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
