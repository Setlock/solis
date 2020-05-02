using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Unity.Burst;

public class Planet : MonoBehaviour
{
    public PlanetSettings planetSettings;
    public Material terrainMat;
    [HideInInspector]
    public bool settingsFoldout;
    [HideInInspector]
    public Texture2D planetTexture;

    public Texture2D terrainSpritemap;
    public DefaultAsset tilesetLookupFile, tilesetTriangulationFile;
    public TilesetLookup tilesetLookup;

    public GameObject liquid;

    Dictionary<Vector2, TerrainChunk> chunkDictionary = new Dictionary<Vector2, TerrainChunk>();

    SimplexNoiseGenerator noise;
    public void Start()
    {
        Init();
    }
    public void StaticBatch()
    {
        StaticBatchingUtility.Combine(gameObject);
    }
    Dictionary<Vector2, TileData> defaultTileData = new Dictionary<Vector2, TileData>();
    /// <summary>
    /// Create initial planet components including tilesetlookup, noise object, and texture
    /// </summary>
    public void Init()
    {
        tilesetLookup = new TilesetLookup(AssetDatabase.GetAssetPath(tilesetLookupFile), AssetDatabase.GetAssetPath(tilesetTriangulationFile));
        noise = new SimplexNoiseGenerator(planetSettings.planetSeed);
        planetTexture = new Texture2D(400, 400);
        //CreateDefaultTileData();
        CreatePlanetTexture();
    }
    public void CreateDefaultTileData()
    {
        int vi = 0;
        int xPos = 0, yPos = 0;
        int length = (planetSettings.chunkWidth * planetSettings.chunkHeight) * 4;
        while (vi < length)
        {
            defaultTileData.Add(new Vector2(xPos, yPos), new TileData(vi, true));
            vi += 4;

            xPos++;
            if (xPos >= planetSettings.chunkWidth)
            {
                yPos++;
                xPos = 0;
            }
        }
        for (int i = -1; i <= planetSettings.chunkWidth; i++)
        {
            //Calculate noise position at x for top and bottom row
            defaultTileData.Add(new Vector2(i, -1), new TileData(-1, true));
            defaultTileData.Add(new Vector2(i, planetSettings.chunkHeight), new TileData(-1, true));
        }
        for (int j = 0; j < planetSettings.chunkHeight; j++)
        {
            defaultTileData.Add(new Vector2(-1, j), new TileData(-1, true));
            defaultTileData.Add(new Vector2(planetSettings.chunkWidth, j), new TileData(-1, true));
        }
        for (int x = 0; x < planetSettings.chunkWidth; x++)
        {
            for (int y = 0; y < planetSettings.chunkHeight; y++)
            {
                Vector2 key = new Vector2(x, y);
                TileData tile = defaultTileData[key];
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
        if (defaultTileData.ContainsKey(position))
        {
            tileData = defaultTileData[position];
        }
        return tileData;
    }
    /// <summary>
    /// Generate planet after planet has already been created
    /// </summary>
    public void Regenerate()
    {
        SetAllChunksToRemove();
        AddChunksToRemoveList();
        RemoveRequiredChunksFromDictionary();

        Init();
    }
    /// <summary>
    /// Generate the planets texture based on given values
    /// </summary>
    public void CreatePlanetTexture()
    {
        //Loop through all pixels setting pixel to planet color based on noise function
        for(int i = 0; i < planetTexture.width; i++)
        {
            for(int j = 0; j < planetTexture.height; j++)
            {
                float val = (float)noise.Evaluate((i-planetTexture.width/2)/planetSettings.planetFeatureSize,(j-planetTexture.height/2)/planetSettings.planetFeatureSize,0);
                if(val >= -0.4f)
                {
                    planetTexture.SetPixel(i,j, planetSettings.terrainColor);
                }
                else
                {
                    planetTexture.SetPixel(i, j, planetSettings.waterColor);
                }
            }
        
        }
        //Set middle points to black to provide reference to start location
        planetTexture.SetPixel(planetTexture.width / 2 - 1, planetTexture.height / 2, new Color(0, 0, 0, 1));
        planetTexture.SetPixel(planetTexture.width / 2 + 1, planetTexture.height / 2, new Color(0, 0, 0, 1));

        planetTexture.SetPixel(planetTexture.width / 2, planetTexture.height / 2, new Color(0, 0, 0, 1));

        planetTexture.SetPixel(planetTexture.width / 2, planetTexture.height / 2 - 1, new Color(0, 0, 0, 1));
        planetTexture.SetPixel(planetTexture.width / 2, planetTexture.height / 2 + 1, new Color(0, 0, 0, 1));

        //Apply changes to pixels
        planetTexture.Apply();
    }
    /// <summary>
    /// Sets all chunks to be removed
    /// </summary>
    public void SetAllChunksToRemove()
    {
        foreach(TerrainChunk c in chunkDictionary.Values)
        {
            c.SetRemove(true);
        }
    }
    /// <summary>
    /// Returns chunk at given location
    /// </summary>
    public TerrainChunk GetChunk(Vector2 position)
    {
        return chunkDictionary[position];
    }
    /// <summary>
    /// Returns true if chunkDictionary contains chunk at location
    /// </summary>
    public bool ContainsChunk(Vector2 position)
    {
        return chunkDictionary.ContainsKey(position);
    }
    /// <summary>
    /// Creates new chunk at given coordinate, generates its mesh, and adds it to chunkDictionary
    /// </summary>
    public void CreateChunk(Vector2 chunkCoord, Vector2 viewChunkCoord, Vector3[] vertices)
    {
        TerrainChunk chunk = new TerrainChunk(terrainMat, noise, transform, vertices, new Vector2[vertices.Length], terrainSpritemap, tilesetLookup, chunkCoord, viewChunkCoord, planetSettings.chunkWidth, planetSettings.chunkHeight, planetSettings.planetFeatureSize, new Color[vertices.Length], planetSettings.terrainColor);
        chunk.GenerateMesh();
        chunkDictionary.Add(chunkCoord, chunk);
        //newChunks.Add(chunkCoord, chunk);

        StaticBatch();
    }
    public void GenerateAllChunkMeshes()
    {
        foreach(TerrainChunk c in chunkDictionary.Values)
        {
            if (!c.IsLoaded())
            {
                c.GenerateMesh();
            }
        }
    }
    Dictionary<Vector2, TerrainChunk> newChunks = new Dictionary<Vector2, TerrainChunk>();
    public void GenerateAllChunkData()
    {
        if (newChunks.Count > 0)
        {
            NativeList<JobHandle> jobHandleList = new NativeList<JobHandle>(Allocator.Temp);
            List<GenerateChunkJob> jobList = new List<GenerateChunkJob>();
            foreach (TerrainChunk c in newChunks.Values)
            {
                GenerateChunkJob chunkJob = new GenerateChunkJob
                {
                    pos = new float3
                    {
                        x = c.position.x,
                        y = c.position.y,
                        z = 0
                    },
                    chunkWidth = c.width,
                    chunkHeight = c.height,
                    featureSize = c.featureSize,
                    tileStateList = new NativeList<bool>(Allocator.Persistent),
                };
                jobList.Add(chunkJob);
                JobHandle jobHandle = chunkJob.Schedule();
                jobHandleList.Add(jobHandle);
            }
            JobHandle.CompleteAll(jobHandleList);
            Vector2 chunkPos = new Vector2();
            foreach (GenerateChunkJob job in jobList)
            {
                chunkPos.x = job.pos.x;
                chunkPos.y = job.pos.y;
                TerrainChunk c = newChunks[chunkPos];
                foreach (bool state in job.tileStateList)
                {
                    c.stateList.Add(state);
                }
                c.CreateTileStatesFromList();
            }

            jobHandleList.Dispose();
            jobList.Clear();
            newChunks.Clear();
        }
    }
    /// <summary>
    /// Remove chunk from dictionary at given location
    /// </summary>
    public void RemoveChunkFromDictionary(Vector2 position)
    {
        if (ContainsChunk(position))
        {
            chunkDictionary.Remove(position);
        }
    }
    List<Vector2> keysToRemove = new List<Vector2>();
    /// <summary>
    /// Adds chunks to list of chunks to remove when RemoveRequiredChunksFromDictionary() is called
    /// </summary>
    public void AddChunksToRemoveList()
    {
        //Clears previous chunks to be removed
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
    }
    /// <summary>
    /// Removes chunk from dictionary based on values from AddChunksToRemoveList()
    /// </summary>
    public void RemoveRequiredChunksFromDictionary()
    {
        foreach (Vector2 key in keysToRemove)
        {
            RemoveChunkFromDictionary(key);
        }
    }
}
[BurstCompile]
public struct GenerateChunkJob : IJob
{
    public float chunkWidth, chunkHeight, featureSize;
    public float3 pos;
    public NativeList<bool> tileStateList;
    //NativeList<float2> uvList;
    public void Execute()
    {
        //chunkNoise = new SimplexNoiseGenerator();
        float3 noisePos = new float3 { };
        int index = 0;
        for(int x = -1; x <= chunkWidth; x++)
        {
            for(int y = -1; y <= chunkHeight; y++)
            {
                noisePos.x = (pos.x + x)/featureSize;
                noisePos.y = (pos.y + y)/featureSize;

                float val = Unity.Mathematics.noise.snoise(noisePos);
                if(val >= -0.4f)
                {
                    tileStateList.Add(true);
                }
                else
                {
                    tileStateList.Add(false);
                }
                index++;
            }
        }
    }
}
