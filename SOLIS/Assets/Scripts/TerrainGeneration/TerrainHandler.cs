using System.Collections.Generic;
using UnityEngine;

public class TerrainHandler : MonoBehaviour
{
    public Transform viewer;
    public Planet planet;
    int viewChunkWidth, viewChunkHeight;
    [Range(1, 20)]
    public int xViewDist = 10, yViewDist = 10;
    DefaultLoader defaultLoader;

    private void Start()
    {
        Init();
    }
    private void Update()
    {
        UpdatePlanetRender(planet);
    }

    private void Init()
    {
        defaultLoader = GetComponent<DefaultLoader>();
        viewChunkWidth = defaultLoader.chunkWidth * defaultLoader.tileWidth;
        viewChunkHeight = defaultLoader.chunkHeight * defaultLoader.tileHeight;
    }
    /// <summary>
    /// Updates given planet chunkDictionary to contain chunks in viewChunkWidth and viewChunkHeight
    /// </summary>
    public void UpdatePlanetRender(Planet p)
    {
        //Calculate x and y position of viewer based on chunk scale
        int currentX = Mathf.RoundToInt(viewer.position.x / (float)viewChunkWidth);
        int currentY = Mathf.RoundToInt(viewer.position.y / (float)viewChunkHeight);

        //Assume all chunks should be removed
        p.SetAllChunksToRemove();

        for (int x = -xViewDist; x < xViewDist; x++)
        {
            for (int y = -yViewDist; y < yViewDist; y++)
            {
                //Get chunk coordinate and chunk true view position coordinate
                Vector2 chunkCoord = new Vector2((currentX + x) * defaultLoader.chunkWidth, (currentY + y) * defaultLoader.chunkHeight);
                Vector2 viewChunkCoord = new Vector2((currentX + x) * viewChunkWidth, (currentY + y) * viewChunkHeight);

                //If chunk with this coordinate has previously been added set shouldRemove = false and if chunk is not loaded, load chunk
                //Else create new chunk at this positon and add to list of all chunks
                if (p.ContainsChunk(chunkCoord))
                {
                    //Get previously created terrain chunk
                    TerrainChunk tc = p.GetChunk(chunkCoord);
                    //Chunk should not be removed
                    tc.SetRemove(false);
                    //Check if chunk is not loaded
                    /*if (!tc.IsLoaded())
                    {
                        tc.GenerateChunk();
                    }*/
                }
                else
                {
                    //Create new chunk at coordinate and add to planet chunkDictionary
                    p.CreateChunk(chunkCoord, defaultLoader.chunkWidth, defaultLoader.chunkHeight, viewChunkCoord, defaultLoader.defaultVertices, defaultLoader.defaultUV, defaultLoader.defaultColor, new Dictionary<Vector2, TileData>(defaultLoader.defaultTileData));
                }
            }
        }
        p.GenerateAllNewChunks(defaultLoader.chunkWidth,defaultLoader.chunkHeight);

        //Calculate chunks to be removed
        p.AddChunksToRemoveList();

        //Removed calculated chunks
        p.RemoveRequiredChunksFromDictionary();
    }
}