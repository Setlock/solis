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
    /// <summary>
    /// Create initial planet components including tilesetlookup, noise object, and texture
    /// </summary>
    public void Init()
    {
        tilesetLookup = new TilesetLookup(AssetDatabase.GetAssetPath(tilesetLookupFile), AssetDatabase.GetAssetPath(tilesetTriangulationFile));
        noise = new SimplexNoiseGenerator(planetSettings.planetSeed);
        planetTexture = new Texture2D(400, 400);
        CreatePlanetTexture();
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
    public void CreateChunk(Vector2 chunkCoord, int chunkWidth, int chunkHeight, Vector2 viewChunkCoord, Vector3[] vertices, Vector2[] uv, Color[] defaultColor, Dictionary<Vector2,TileData> tileData)
    {
        TerrainChunk chunk = new TerrainChunk(terrainMat, noise, transform, vertices, uv, tileData, terrainSpritemap, tilesetLookup, chunkCoord, viewChunkCoord, chunkWidth, chunkHeight, planetSettings.planetFeatureSize, defaultColor, planetSettings.terrainColor);
        chunk.GenerateChunk();
        chunkDictionary.Add(chunkCoord, chunk);
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
