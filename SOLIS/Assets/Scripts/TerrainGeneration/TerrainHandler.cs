using System.Collections.Generic;
using UnityEngine;

public class TerrainHandler : MonoBehaviour
{
    public Transform viewer;
    public Planet planet;
    int viewChunkWidth, viewChunkHeight;
    [Range(1, 20)]
    public int xViewDist = 10, yViewDist = 10;
    Vector3[] vertices;

    private void Start()
    {
        //Create Planet
        Init(planet);
    }
    private void FixedUpdate()
    {
        UpdatePlanetRender(planet);
    }

    /// <summary>
    /// Create vertices for planet and set local view variables
    /// </summary>
    private void Init(Planet p)
    {
        int chunkWidth = p.planetSettings.chunkWidth;
        int chunkHeight = p.planetSettings.chunkHeight;

        int tileWidth = p.planetSettings.tileWidth;
        int tileHeight = p.planetSettings.tileHeight;

        vertices = new Vector3[chunkWidth * chunkHeight * 4];

        viewChunkWidth = chunkWidth * tileWidth;
        viewChunkHeight = chunkHeight * tileHeight;
        for (int i = 0, y = 0; y < viewChunkHeight; y += tileWidth)
        {
            for (int x = 0; x < viewChunkWidth; x += tileHeight)
            {
                //Bottom Left
                vertices[i] = new Vector3(x, y);

                //Bottom Right
                vertices[i + 1] = new Vector3(x + tileWidth, y);

                //Top Left
                vertices[i + 2] = new Vector3(x, y + tileHeight);

                //Top Right
                vertices[i + 3] = new Vector3(x + tileWidth, y + tileHeight);

                //Increment index
                i += 4;
            }
        }
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
                Vector2 chunkCoord = new Vector2((currentX + x) * p.planetSettings.chunkWidth, (currentY + y) * p.planetSettings.chunkHeight);
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
                    if (!tc.IsLoaded())
                    {
                        tc.GenerateMesh();
                    }
                }
                else
                {
                    //Create new chunk at coordinate and add to planet chunkDictionary
                    p.CreateChunk(chunkCoord, viewChunkCoord, vertices);
                }
            }
        }
        //Calculate chunks to be removed
        p.AddChunksToRemoveList();

        //Removed calculated chunks
        p.RemoveRequiredChunksFromDictionary();
    }
}