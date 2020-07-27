using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;

public class TerrainHandler : MonoBehaviour
{
    public Transform viewer;
    public Material terrainMat;
    public int planetWidth, planetHeight;
    public Vector2 chunkDimensions;
    public int seed, numLayers;
    public float noiseCutoff, featureSize, recede, baseRoughness, roughness, persistence, strength;

    public int viewDistX, viewDistY;

    Queue<GameObject> chunkPool = new Queue<GameObject>();
    Dictionary<Vector2, TerrainChunk> chunks = new Dictionary<Vector2, TerrainChunk>();
    Dictionary<Vector2, ChunkData> terrainData = new Dictionary<Vector2, ChunkData>();

    public GameObject orePrefab;

    NativeHashMap<float2, JobHandle> stateJobs, meshJobs;
    private void Start()
    {
        viewer.transform.position = new Vector3(planetWidth / 2f, planetHeight / 2, viewer.transform.position.z);
        stateJobs = new NativeHashMap<float2, JobHandle>(100, Allocator.Persistent);
        meshJobs = new NativeHashMap<float2, JobHandle>(100, Allocator.Persistent);
    }
    private void OnValidate()
    {
        if (GUI.changed)
        {
            Regenerate();
        }
    }
    public void Regenerate()
    {
        foreach(TerrainChunk chunk in chunks.Values)
        {
            chunk.SetRemove(true);
        }
        UpdateJobs();
        foreach(Vector2 key in chunks.Keys)
        {
            stateJobs.Add(key,chunks[key].CreateStateJob(seed, numLayers, featureSize, recede, baseRoughness, roughness, persistence, strength));
            chunks[key].SetRemove(false);
        }
    }
    private void Update()
    {
        foreach(TerrainChunk chunk in chunks.Values)
        {
            chunk.SetRemove(true);
        }

        int currentX = Mathf.RoundToInt(viewer.position.x / chunkDimensions.x);
        int currentY = Mathf.RoundToInt(viewer.position.y / chunkDimensions.y);

        for(int x = -viewDistX; x < viewDistX; x++)
        {
            for(int y = -viewDistY; y < viewDistY; y++)
            {
                Vector2 chunkCoord = new Vector2((currentX + x), (currentY + y));
                if (chunks.ContainsKey(chunkCoord))
                {
                    TerrainChunk terrainChunk = chunks[chunkCoord];
                    terrainChunk.SetRemove(false);
                }
                else
                {
                    Vector2 worldChunkCoord = new Vector2(chunkCoord.x * chunkDimensions.x, chunkCoord.y * chunkDimensions.y);
                    Vector2 sampleCoord = new Vector2(worldChunkCoord.x, worldChunkCoord.y);
                    if(worldChunkCoord.x > 0)
                    {
                        sampleCoord.x -= planetWidth * (int)(sampleCoord.x / planetWidth);
                    }
                    if (worldChunkCoord.y > 0)
                    {
                        sampleCoord.y -= planetHeight * (int)(sampleCoord.y / planetHeight);
                    }

                    if (worldChunkCoord.x < 0)
                    {
                        sampleCoord.x += planetWidth * ((int)(-sampleCoord.x / planetWidth) + 1);
                    }
                    if (worldChunkCoord.y < 0)
                    {
                        sampleCoord.y += planetHeight * ((int)(-sampleCoord.y / planetHeight) + 1);
                    }

                    ChunkData chunkData;
                    if(terrainData.ContainsKey(sampleCoord))
                    {
                        chunkData = terrainData[sampleCoord];
                    }
                    else
                    {
                        chunkData = new ChunkData();
                        terrainData.Add(sampleCoord, chunkData);
                    }

                    TerrainChunk tc;
                    if(chunkPool.Count > 0)
                    {
                        GameObject chunkObject = chunkPool.Dequeue();
                        chunkObject.name = "Terrain Chunk" + sampleCoord;

                        tc = new TerrainChunk(chunkData, this, chunkObject, sampleCoord, worldChunkCoord, chunkDimensions);
                        chunks.Add(chunkCoord, tc);
                    }
                    else
                    {
                        GameObject newChunkObject = new GameObject("Terrain Chunk" + sampleCoord, typeof(MeshFilter), typeof(MeshRenderer));
                        newChunkObject.transform.SetParent(transform, true);

                        tc = new TerrainChunk(chunkData, this, newChunkObject, sampleCoord, worldChunkCoord, chunkDimensions);
                        chunks.Add(chunkCoord, tc);
                    }
                    stateJobs.Add(chunkCoord, tc.CreateStateJob(seed, numLayers, featureSize, recede, baseRoughness, roughness, persistence, strength));
                }
            }
        }

        UpdateJobs();

        Vector2[] keys = chunks.Keys.ToArray();
        foreach(Vector2 key in keys)
        {
            if (chunks[key].ShouldRemove())
            {
                chunkPool.Enqueue(chunks[key].Remove());
                chunks.Remove(key);
            }
        }
    }
    private void UpdateJobs()
    {
        if (stateJobs.Count() > 0)
        {
            NativeArray<float2> keys = stateJobs.GetKeyArray(Allocator.Temp);
            foreach (float2 key in keys)
            {
                if (stateJobs[key].IsCompleted || chunks[key].ShouldRemove())
                {
                    chunks[key].CompleteStateJob(stateJobs[key]);
                    meshJobs.Add(key, chunks[key].CreateMeshJob());
                    stateJobs.Remove(key);
                }
            }
            keys.Dispose();
        }
        if(meshJobs.Count() > 0)
        {
            NativeArray<float2> keys = meshJobs.GetKeyArray(Allocator.Temp);
            foreach (float2 key in keys)
            {
                if (meshJobs[key].IsCompleted || chunks[key].ShouldRemove())
                {
                    chunks[key].CompleteMeshJob(meshJobs[key]);
                    meshJobs.Remove(key);
                }
            }
            keys.Dispose();
        }
    }
    private void OnApplicationQuit()
    {
        stateJobs.Dispose();
        meshJobs.Dispose();
    }

}
