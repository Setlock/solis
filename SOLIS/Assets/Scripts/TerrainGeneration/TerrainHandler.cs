using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TerrainHandler : MonoBehaviour
{
    public Transform viewer;
    [Range(1,50)]
    public int chunkWidth = 10, chunkHeight = 10;
    [Range(-50,-1)]
    public int startX = -1, startY = -1;
    public float featureSize = 16;

    Dictionary<Vector2, TerrainChunk> chunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    public int xViewDist = 10, yViewDist = 10;
    void Update()
    {
        //Calculate current x and y position of player
        int currentX = Mathf.RoundToInt(viewer.position.x / (float)chunkWidth);
        int currentY = Mathf.RoundToInt(viewer.position.y / (float)chunkHeight);

        //Assume all chunks should be removed
        foreach(TerrainChunk c in chunkDictionary.Values)
        {
            c.SetRemove(true);
        }

        //Iterate through all chunks within arbitary view dist (this will be changed later to correctly calculate camera view size)
        for(int x = -xViewDist; x < xViewDist; x++)
        {
            for(int y = -yViewDist; y < yViewDist; y++)
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
                    TerrainChunk chunk = new TerrainChunk(transform, chunkCoord, chunkWidth, chunkHeight, featureSize);
                    chunkDictionary.Add(chunkCoord, chunk);
                }
            }
        }

        //Loop through all chunks in list. If chunk should be removed then destroy the GameObject containing its mesh
        //and set chunk loaded = false
        foreach (TerrainChunk c in chunkDictionary.Values)
        {
            if (c.ShouldRemove())
            {
                Destroy(c.GetGameObject());
                c.Remove();
            }
        }
    }
}
