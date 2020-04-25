using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TerrainHandler : MonoBehaviour
{
    public Transform viewer;
    public Texture2D terrainSpritemap;
    [Range(1,50)]
    public int chunkWidth = 10, chunkHeight = 10;
    [Range(-50,-1)]
    public int startX = -1, startY = -1;
    public float featureSize = 16;

    Dictionary<Vector2, TerrainChunk> chunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    [Range(1,20)]
    public int xViewDist = 10, yViewDist = 10;

    TilesetLookup tilesetLookup;
    public DefaultAsset tilesetLookupFile, tilesetTriangulationFile;
    Vector3[] vertices;
    Vector2[] uv;
    // Temporary method used for creating vertices and indices for chunks
    private void Start()
    {
        tilesetLookup = new TilesetLookup(AssetDatabase.GetAssetPath(tilesetLookupFile),AssetDatabase.GetAssetPath(tilesetTriangulationFile));
        /*Calculate texture to use (64 and 96 are arbitrary values)
        float textureOffsetX = 64f / (float)terrainSpritemap.width;
        float textureOffsetY = 64f / (float)terrainSpritemap.height;

        //Set width of tiles based on tilemap
        float tileWidth = 64 / (float)terrainSpritemap.width;
        float tileHeight = 64 / (float)terrainSpritemap.height;*/

        //Create empty vertices and uv with size of (Chunk Area) * (Number of vertices per quad)
        //NOTE: UVS NOT SET TO IMPROVE STARTUP TIME-THIS MAY CHANGE IN THE FUTURE
        vertices = new Vector3[chunkWidth * chunkHeight * 4];
        uv = new Vector2[vertices.Length];
        for (int i = 0, y = 0; y < chunkHeight; y++)
        {
            for (int x = 0; x < chunkWidth; x++)
            {
                //Bottom Left
                vertices[i] = new Vector3(x, y);
                //uv[i] = new Vector2(textureOffsetX, textureOffsetY);

                //Bottom Right
                vertices[i + 1] = new Vector3(x + 1, y);
                //uv[i + 1] = new Vector2(textureOffsetX + tileWidth, textureOffsetY);

                //Top Left
                vertices[i + 2] = new Vector3(x, y + 1);
                //uv[i + 2] = new Vector2(textureOffsetX, textureOffsetY + tileHeight);

                //Top Right
                vertices[i + 3] = new Vector3(x + 1, y + 1);
                //uv[i + 3] = new Vector2(textureOffsetX + tileWidth, textureOffsetY + tileHeight);

                //Increment index
                i += 4;
            }
        }
    }
    List<Vector2> keysToRemove = new List<Vector2>();
    private void FixedUpdate()
    {
        //Calculate current x and y position of player
        int currentX = Mathf.RoundToInt(viewer.position.x / (float)chunkWidth);
        int currentY = Mathf.RoundToInt(viewer.position.y / (float)chunkHeight);

        //Assume all chunks should be removed
        foreach (TerrainChunk c in chunkDictionary.Values)
        {
            c.SetRemove(true);
        }

        //Iterate through all chunks within arbitary view dist (this will be changed later to correctly calculate camera view size)
        for (int x = -xViewDist; x < xViewDist; x++)
        {
            for (int y = -yViewDist; y < yViewDist; y++)
            {
                //Get chunk coordinate
                Vector2 chunkCoord = new Vector2((currentX + x) * chunkWidth, (currentY + y) * chunkHeight);
                //If chunk with this coordinate has previously been added set shouldRemove = false and if chunk is not loaded, load chunk
                //Else create new chunk at this positon and add to list of all chunks
                if (chunkDictionary.ContainsKey(chunkCoord))
                {
                    //Get previously created terrain chunk
                    TerrainChunk tc = chunkDictionary[chunkCoord];
                    //Chunk should not be removed
                    tc.SetRemove(false);
                    //Check if chunk is not loaded
                    if (!tc.IsLoaded())
                    {
                        tc.GenerateMesh();
                    }
                }
                else
                {
                    TerrainChunk chunk = new TerrainChunk(transform, vertices, uv, terrainSpritemap, tilesetLookup, chunkCoord, chunkWidth, chunkHeight, featureSize);
                    //Generate newly created terrain chunk
                    chunk.GenerateMesh();
                    chunkDictionary.Add(chunkCoord, chunk);
                }
            }
        }
        //Loop through all chunks in list. If chunk should be removed then disable the GameObject 
        //containing its mesh, set loaded = false, and add key to remove chunk from chunkDictionary
        keysToRemove.Clear();
        foreach (Vector2 key in chunkDictionary.Keys)
        {
            TerrainChunk c = chunkDictionary[key];
            if (c.ShouldRemove())
            {
                c.Remove();
                keysToRemove.Add(key);
            }
        }
        //Remove terrain chunk from chunkDictionary to save memory
        foreach (Vector2 key in keysToRemove)
        {
            chunkDictionary.Remove(key);
        }
    }
}
