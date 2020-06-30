using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

public class TerrainHandler : MonoBehaviour
{
    public Transform viewer;
    public Material terrainMat;
    public int planetWidth, planetHeight;
    public Vector2 chunkDimensions, tileSize;
    public int seed, numLayers;
    public float noiseCutoff, featureSize, recede, baseRoughness, roughness, persistence, strength;

    public int viewDistX, viewDistY;

    Queue<GameObject> chunkPool = new Queue<GameObject>();
    Dictionary<Vector2, TerrainChunk> chunks = new Dictionary<Vector2, TerrainChunk>();

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
                    chunks[chunkCoord].SetRemove(false);
                }
                else
                {
                    Vector2 worldChunkCoord = new Vector2(chunkCoord.x * chunkDimensions.x, chunkCoord.y * chunkDimensions.y);
                    Vector2 sampleCoord = new Vector2(worldChunkCoord.x, worldChunkCoord.y);

                    if(sampleCoord.x > 0 && sampleCoord.x % planetWidth == 0)
                    {
                        sampleCoord.x -= planetWidth * (int)(sampleCoord.x / planetWidth);
                    }
                    if (sampleCoord.y > 0 && sampleCoord.y % planetHeight == 0)
                    {
                        sampleCoord.y -= planetHeight * (int)(sampleCoord.y / planetHeight);
                    }

                    if (sampleCoord.x < 0 && (-1f * (sampleCoord.x + chunkDimensions.x)) % planetWidth == 0)
                    {
                        sampleCoord.x += planetWidth * ((int)(sampleCoord.x / planetWidth) + 1);
                    }
                    if (sampleCoord.y < 0 && (-1f * (sampleCoord.y + chunkDimensions.y)) % planetHeight == 0)
                    {
                        sampleCoord.y += planetHeight * ((int)(sampleCoord.y / planetHeight) + 1);
                    }



                    TerrainChunk tc;
                    if(chunkPool.Count > 0)
                    {
                        tc = new TerrainChunk(this, chunkPool.Dequeue(), sampleCoord, worldChunkCoord, chunkDimensions, tileSize);
                        chunks.Add(chunkCoord, tc);
                    }
                    else
                    {
                        GameObject newChunkObject = new GameObject("Terrain Chunk" + worldChunkCoord, typeof(MeshFilter), typeof(MeshRenderer));
                        newChunkObject.transform.SetParent(transform, true);

                        tc = new TerrainChunk(this, newChunkObject, sampleCoord, worldChunkCoord, chunkDimensions, tileSize);
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
[BurstCompile]
public struct ValueJob : IJobParallelFor
{
    public int planetWidth;
    public int seed, numLayers;
    public float featureSize, recede, baseRoughness, roughness, persistence, strength;
    public float noiseCutoff;

    public int2 chunkPosition;
    public int2 chunkDimensions;
    public NativeArray<bool> states;
    public void Execute(int index)
    {
        float2 position = new float2((index / (chunkDimensions.x+2)) - 1, (index % (chunkDimensions.x+2)) - 1);
        position += chunkPosition;
        states[index] = GetNoiseState(position);
    }
    float GetNoiseValue(float2 position)
    {
        float twoPi = math.PI * 2;

        float xVal = math.sin(position.y * twoPi / (float)planetWidth) / twoPi * (float)planetWidth;
        float yVal = math.cos(position.y * twoPi / (float)planetWidth) / twoPi * (float)planetWidth;
        float zVal = math.sin(position.x * twoPi / (float)planetWidth) / twoPi * (float)planetWidth;
        float wVal = math.cos(position.x * twoPi / (float)planetWidth) / twoPi * (float)planetWidth;

        float4 samplePosition = new float4(xVal,yVal,zVal,wVal);
        samplePosition += seed;

        float noiseValue = 0;
        float frequency = baseRoughness;
        float amplitude = 1;

        for (int i = 0; i < numLayers; i++)
        {
            float v = noise.snoise(samplePosition / featureSize * frequency);
            noiseValue += (v + 1) * 0.5f * amplitude;
            frequency *= roughness;
            amplitude *= persistence;
        }

        float finalValue = Mathf.Max(0, noiseValue - recede);

        return finalValue * strength;
    }
    bool GetNoiseState(float2 position)
    {
        if (GetNoiseValue(position) < noiseCutoff)
        {
            return true;
        }
        return false;
    }
}
[BurstCompile]
public struct MeshJob : IJob
{
    public NativeArray<bool> states;

    public int2 chunkDimensions, tileSize;
    public NativeList<float3> vertices;
    public NativeList<float2> uvs;
    public NativeList<int> triangles;
    public void Execute()
    {
        int triangleIndex = 0;
        for(int i = 0; i < states.Length; i++)
        {
            float2 position = new float2((i / (chunkDimensions.x + 2)) - 1, (i % (chunkDimensions.x + 2)) - 1);
            if (position.x >= 0 && position.x < chunkDimensions.x && position.y >= 0 && position.y < chunkDimensions.y)
            {
                if (states[i])
                {
                    float3 v1 = new float3(position.x, position.y, 0);
                    float3 v2 = new float3(position.x + tileSize.x, position.y, 0);
                    float3 v3 = new float3(position.x, position.y + tileSize.y, 0);
                    float3 v4 = new float3(position.x + tileSize.x, position.y + tileSize.y, 0);

                    NativeArray<float2> uvArr = GetUV(position);
                    for(int j = 0; j < 4; j++) 
                    {
                        uvs.Add(uvArr[j]);
                    }
                    uvArr.Dispose();

                    int t1 = triangleIndex;
                    int t2 = triangleIndex + 2;
                    int t3 = triangleIndex + 1;

                    int t4 = triangleIndex + 1;
                    int t5 = triangleIndex + 2;
                    int t6 = triangleIndex + 3;

                    vertices.Add(v1);
                    vertices.Add(v2);
                    vertices.Add(v3);
                    vertices.Add(v4);

                    triangles.Add(t1);
                    triangles.Add(t2);
                    triangles.Add(t3);
                    triangles.Add(t4);
                    triangles.Add(t5);
                    triangles.Add(t6);

                    triangleIndex += 4;
                }
            }
        }
    }
    private NativeArray<float2> GetUV(float2 position)
    {
        NativeArray<float2> uvArr = new NativeArray<float2>(4, Allocator.Temp);
        bool tileTL, tileT, tileTR, tileL, tileR, tileBL, tileB, tileBR;
        tileTL = states[GetIndex(new float2(position.x - 1, position.y + 1))];
        tileT = states[GetIndex(new float2(position.x, position.y + 1))];
        tileTR = states[GetIndex(new float2(position.x + 1, position.y + 1))];

        tileL = states[GetIndex(new float2(position.x - 1, position.y))];
        tileR = states[GetIndex(new float2(position.x + 1, position.y))];

        tileBL = states[GetIndex(new float2(position.x - 1, position.y - 1))];
        tileB = states[GetIndex(new float2(position.x, position.y - 1))];
        tileBR = states[GetIndex(new float2(position.x + 1, position.y - 1))];
        TerrainType tileName = TerrainType.Grass;
        if (!tileT)
        {
            tileName = TerrainType.TopEdge;
            if (!tileR)
            {
                tileName = TerrainType.CornerTR;
            }
            else if (!tileL)
            {
                tileName = TerrainType.CornerTL;
            }
        }
        else if (!tileB)
        {
            tileName = TerrainType.BottomEdge;
            if (!tileR)
            {
                tileName = TerrainType.CornerBR;
            }
            else if (!tileL)
            {
                tileName = TerrainType.CornerBL;
            }
        }
        else if (!tileR)
        {
            tileName = TerrainType.RightEdge;
        }
        else if (!tileL)
        {
            tileName = TerrainType.LeftEdge;
        }
        else if (!tileBR)
        {
            tileName = TerrainType.CurveTL;
        }
        else if (!tileTR)
        {
            tileName = TerrainType.CurveBL;
        }
        else if (!tileBL)
        {
            tileName = TerrainType.CurveTR;
        }
        else if (!tileTL)
        {
            tileName = TerrainType.CurveBR;
        }
        float2 uvCoord = GetCoord(tileName);
        uvArr[0] = uvCoord;
        uvArr[1] = uvCoord + new float2(16f / 80f, 0);
        uvArr[2] = uvCoord + new float2(0, 16f/ 48f);
        uvArr[3] = uvCoord + new float2(16f / 80f, 16f / 48f);

        return uvArr;
    }
    private int GetIndex(float2 position)
    {
        return (int)((position.x+1) * (chunkDimensions.x+2) + (position.y+1));
    }
    private float2 GetCoord(TerrainType name)
    {
        float2 uv = new float2(16f / 80f,  16f / 48f);
        if (name == TerrainType.CornerBL)
        {
            uv.x = 0 / 80f;
            uv.y = 0 / 48f;
        }
        if (name == TerrainType.BottomEdge)
        {
            uv.x = 16 / 80f;
            uv.y = 0 / 48f;
        }
        if (name == TerrainType.CornerBR)
        {
            uv.x = 32 / 80f;
            uv.y = 0 / 48f;
        }

        if (name == TerrainType.LeftEdge)
        {
            uv.x = 0 / 80f;
            uv.y = 16 / 48f;
        }
        if (name == TerrainType.RightEdge)
        {
            uv.x = 32 / 80f;
            uv.y = 16 / 48f;
        }

        if (name == TerrainType.CornerTL)
        {
            uv.x = 0 / 80f;
            uv.y = 32 / 48f;
        }
        if (name == TerrainType.TopEdge)
        {
            uv.x = 16 / 80f;
            uv.y = 32 / 48f;
        }
        if (name == TerrainType.CornerTR)
        {
            uv.x = 32 / 80f;
            uv.y = 32 / 48f;
        }

        if(name == TerrainType.CurveTL)
        {
            uv.x = 48 / 80f;
            uv.y = 32 / 48f;
        }
        if (name == TerrainType.CurveTR)
        {
            uv.x = 64 / 80f;
            uv.y = 32 / 48f;
        }
        if (name == TerrainType.CurveBL)
        {
            uv.x = 48 / 80f;
            uv.y = 16 / 48f;
        }
        if (name == TerrainType.CurveBR)
        {
            uv.x = 64 / 80f;
            uv.y = 16 / 48f;
        }
        return uv;
    }
}
public enum TerrainType
{
    Grass,CornerTL,CornerTR,CornerBL,CornerBR,TopEdge,RightEdge,BottomEdge,LeftEdge,CurveTL,CurveTR,CurveBL,CurveBR
}
