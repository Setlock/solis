using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
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
    public TilesetLookup tilesetLookup;
    public TextAsset tilesetInformation, tilesetTriangulation;

    public GameObject liquid;

    Dictionary<Vector2, TerrainChunk> chunkDictionary = new Dictionary<Vector2, TerrainChunk>();

    System.Random random;
    public void Start()
    {
        Init();
    }
    /// <summary>
    /// Create initial planet components including tilesetlookup, noise object, and texture
    /// </summary>
    public void Init()
    {
        random = new System.Random(planetSettings.planetSeed);
        tilesetLookup = new TilesetLookup(tilesetInformation,tilesetTriangulation);
        planetTexture = new Texture2D(400, 400);
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
    public void CombineMeshes()
    {
        GameObject combinedMeshObject = new GameObject("Main Mesh", typeof(MeshRenderer), typeof(MeshFilter));
        Vector3 pos = combinedMeshObject.transform.position;
        combinedMeshObject.transform.position = Vector3.zero;
        CombineInstance[] combine = new CombineInstance[chunkDictionary.Count];
        int index = 0;
        foreach(TerrainChunk c in newChunks.Values)
        {
            combine[index].mesh = c.GetGameObject().GetComponent<MeshFilter>().sharedMesh;
            combine[index].transform = c.GetGameObject().GetComponent<MeshFilter>().transform.localToWorldMatrix;
            c.GetGameObject().SetActive(false);
            index++;
        }
        combinedMeshObject.transform.GetComponent<MeshFilter>().mesh = new Mesh();
        combinedMeshObject.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, true, true);
        combinedMeshObject.transform.gameObject.SetActive(true);

        combinedMeshObject.transform.position = pos;
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
                float4 pos = new float4((i - planetTexture.width / 2) / planetSettings.planetFeatureSize, (j - planetTexture.height / 2) / planetSettings.planetFeatureSize, 0, planetSettings.planetSeed);
                if(PlanetNoise.GetStateFromPos(pos))
                {
                    planetTexture.SetPixel(i,j, planetSettings.terrainColor);
                }
                else
                {
                    planetTexture.SetPixel(i, j, planetSettings.waterColor);
                }
                //planetTexture.SetPixel(i, j, new Color(val, val, val));
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
        TerrainChunk chunk = new TerrainChunk(this, terrainMat, transform, vertices, uv, tileData, terrainSpritemap, tilesetLookup, chunkCoord, viewChunkCoord, chunkWidth, chunkHeight, planetSettings.planetFeatureSize, defaultColor, planetSettings.terrainColor);
        chunkDictionary.Add(chunkCoord, chunk);
        newChunks.Add(chunkCoord, chunk);
    }
    Dictionary<Vector2, TerrainChunk> newChunks = new Dictionary<Vector2, TerrainChunk>();

    List<NativeList<bool>> chunkBoolMap = new List<NativeList<bool>>();
    public void GenerateAllNewChunks(int chunkWidth, int chunkHeight)
    {
        if (newChunks.Count > 0)
        {
            NativeList<JobHandle> jobHandleList = new NativeList<JobHandle>(Allocator.Temp);
            foreach (TerrainChunk c in newChunks.Values)
            {
                //NativeHashMap<float2, bool> stateMap = new NativeHashMap<float2, bool>(chunkWidth * chunkHeight, Allocator.TempJob);
                NativeList<bool> stateList = new NativeList<bool>(chunkWidth*chunkHeight,Allocator.TempJob);
                GenerateChunkJob chunkJob = new GenerateChunkJob
                {
                    width = chunkWidth,
                    height = chunkHeight,
                    position = c.position,
                    seed = planetSettings.planetSeed,
                    featureSize = planetSettings.planetFeatureSize,
                    tileStates = stateList
                };
                chunkBoolMap.Add(stateList);
                jobHandleList.Add(chunkJob.Schedule());
            }
            JobHandle.CompleteAll(jobHandleList);

            int index = 0;

            foreach (TerrainChunk c in newChunks.Values)
            {
                c.SetTileStatesFromNativeList(chunkBoolMap[index]);
                

                c.GenerateChunk();

                chunkBoolMap[index].Dispose();
                index++;
            }
            //CombineMeshes();

            jobHandleList.Dispose();

            chunkBoolMap.Clear();
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
    public int seed;
    public float featureSize;
    public int width, height;

    public float3 position;
    public NativeList<bool> tileStates;

    public void Execute()
    {
        for(int j = -1; j <= height; j++)
        {
            for(int i = -1; i <= width; i++)
            {
                float4 tilePos = new float4((position.x + i) / featureSize, (position.y + j) / featureSize, 0 ,seed);
                tileStates.Add(PlanetNoise.GetStateFromPos(tilePos));
            }
        }
    }
}