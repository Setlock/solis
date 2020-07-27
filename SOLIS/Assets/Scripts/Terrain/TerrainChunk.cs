using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using System.Runtime;

public class TerrainChunk
{
    TerrainHandler handler;
    public ChunkData data;
    public Vector2 worldPosition, samplePos;
    public Vector2 position;
    public Vector2 dimensions;

    public System.Random random;

    GameObject chunkObject;

    bool remove = false;
    public TerrainChunk(ChunkData data, TerrainHandler handler, GameObject chunkObject, Vector2 samplePos, Vector2 worldPosition, Vector2 dimensions)
    {
        this.data = data;

        this.handler = handler;
        this.samplePos = samplePos;
        this.position = worldPosition;
        this.worldPosition = worldPosition;
        this.dimensions = dimensions;

        int id = ((int)(samplePos.x * handler.planetWidth) + (int)samplePos.y);
        random = new System.Random(id*100);

        chunkObject.transform.position = worldPosition;
        this.chunkObject = chunkObject;
    }
    NativeArray<bool> states, oreStates;
    public JobHandle CreateStateJob(int seed, int numLayers, float featureSize, float recede, float baseRoughness, float roughness, float persistence, float strength)
    {
        int2 chunkPosition = new int2((int)position.x, (int)position.y);
        int2 chunkDimensions = new int2((int)(dimensions.x), (int)(dimensions.y));

        int jobSize = (int)((chunkDimensions.x + 2) * (chunkDimensions.y + 2));
        states = new NativeArray<bool>(jobSize, Allocator.Persistent);

        ValueJob valueJob = new ValueJob
        {
            planetWidth = handler.planetWidth,
            noiseCutoff = handler.noiseCutoff,
            seed = seed,
            numLayers = numLayers,
            featureSize = featureSize,
            recede = recede,
            baseRoughness = baseRoughness,
            roughness = roughness,
            persistence = persistence,
            strength = strength,
            chunkPosition = chunkPosition,
            chunkDimensions = chunkDimensions,
            states = states,
        };
        return valueJob.Schedule(jobSize, (int)(dimensions.x + 2));
    }
    public void CompleteStateJob(JobHandle job)
    {
        job.Complete();
    }

    NativeList<float3> vertices;
    NativeList<int> triangles;
    NativeList<float2> uvs, colliders;
    public JobHandle CreateMeshJob()
    {
        vertices = new NativeList<float3>(Allocator.Persistent);
        triangles = new NativeList<int>(Allocator.Persistent);
        uvs = new NativeList<float2>(Allocator.Persistent);

        colliders = new NativeList<float2>(Allocator.Persistent);
        oreStates = new NativeArray<bool>(states.Length, Allocator.Persistent);

        int2 chunkPosition = new int2((int)position.x, (int)position.y);
        int2 chunkDimensions = new int2((int)(dimensions.x), (int)(dimensions.y));
        MeshJob meshJob = new MeshJob
        {
            states = states,
            oreStates = oreStates,
            planetWidth = handler.planetWidth,
            seed = handler.seed,
            chunkPosition = chunkPosition,
            chunkDimensions = chunkDimensions,
            vertices = vertices,
            uvs = uvs,
            triangles = triangles,
            colliders = colliders,
        };

        return meshJob.Schedule();
    }
    public void CompleteMeshJob(JobHandle job)
    {
        job.Complete();

        Vector3[] meshVertices = new Vector3[vertices.Length];
        for(int i = 0; i < vertices.Length; i++)
        {
            meshVertices[i] = vertices[i];
        }
        Vector2[] meshUV = new Vector2[uvs.Length];
        for(int i = 0; i < uvs.Length; i++)
        {
            meshUV[i] = uvs[i];
        }

        for(int i = 0; i < colliders.Length; i++)
        {
            BoxCollider2D collider = chunkObject.AddComponent<BoxCollider2D>();
            collider.offset = colliders[i] + 0.5f;
            collider.size = new Vector2(1,1);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = meshVertices;
        mesh.uv = meshUV;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        chunkObject.GetComponent<MeshFilter>().sharedMesh = mesh;
        chunkObject.GetComponent<MeshRenderer>().sharedMaterial = handler.terrainMat;
        chunkObject.GetComponent<MeshRenderer>().sortingLayerName = "Terrain";
        chunkObject.SetActive(true);

        states.Dispose();
        vertices.Dispose();
        triangles.Dispose();
        uvs.Dispose();
        colliders.Dispose();

        GenerateChunkData();
    }
    public void GenerateChunkData()
    {
        if (data.GetTileDictionary().Count < 1)
        {
            for (int i = 0; i < oreStates.Length; i++)
            {
                if (oreStates[i])
                {
                    float2 position = new float2((i / (dimensions.x + 2)) - 1, (i % (dimensions.x + 2)) - 1);
                    GameObject ore = GameObject.Instantiate(handler.orePrefab, new Vector3(position.x, position.y, 0), Quaternion.identity);
                    ore.transform.SetParent(chunkObject.transform, false);
                    data.AddTile(position, ore);
                }
            }
        }
        else
        {
            foreach(GameObject tileObj in data.GetTileDictionary().Values)
            {
                tileObj.transform.SetParent(chunkObject.transform, false);
                tileObj.SetActive(true);
            }
        }

        oreStates.Dispose();
    }
    public void DestroyColliders()
    {
        Collider2D[] colliders = chunkObject.GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            GameObject.Destroy(collider);
        }
    }
    public GameObject Remove()
    {
        DestroyColliders();
        chunkObject.SetActive(false);
        data.Update();

        foreach(GameObject obj in data.GetTileDictionary().Values)
        {
            obj.SetActive(false);
        }

        return chunkObject;
    }
    public bool ShouldRemove()
    {
        return remove;
    }
    public void SetRemove(bool remove)
    {
        this.remove = remove;
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
        float2 position = new float2((index / (chunkDimensions.x + 2)) - 1, (index % (chunkDimensions.x + 2)) - 1);
        states[index] = GetNoiseState(position + chunkPosition);
    }
    float GetNoiseValue(float2 position)
    {
        float twoPi = math.PI * 2;

        float xVal = math.sin(position.y * twoPi / (float)planetWidth) / twoPi * (float)planetWidth;
        float yVal = math.cos(position.y * twoPi / (float)planetWidth) / twoPi * (float)planetWidth;
        float zVal = math.sin(position.x * twoPi / (float)planetWidth) / twoPi * (float)planetWidth;
        float wVal = math.cos(position.x * twoPi / (float)planetWidth) / twoPi * (float)planetWidth;

        float4 samplePosition = new float4(xVal, yVal, zVal, wVal);
        samplePosition += seed;
        samplePosition /= featureSize;

        float noiseValue = 0;
        float frequency = baseRoughness;
        float amplitude = 1;

        for (int i = 0; i < numLayers; i++)
        {
            float v = noise.snoise(samplePosition * frequency);
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
    public int planetWidth, seed;
    public int2 chunkPosition;

    public NativeArray<bool> states;

    public int2 chunkDimensions;
    public NativeList<float3> vertices;
    public NativeList<float2> uvs;
    public NativeList<int> triangles;

    public NativeList<float2> colliders;
    public NativeArray<bool> oreStates;

    public void Execute()
    {
        int triangleIndex = 0;
        for (int i = 0; i < states.Length; i++)
        {
            float2 position = new float2((i / (chunkDimensions.x + 2)) - 1, (i % (chunkDimensions.x + 2)) - 1);
            if (position.x >= 0 && position.x < chunkDimensions.x && position.y >= 0 && position.y < chunkDimensions.y)
            {
                if (states[i])
                {
                    float2 vPos = position;
                    float3 v1 = new float3(vPos.x, vPos.y, 0);
                    float3 v2 = new float3(vPos.x + 1, vPos.y, 0);
                    float3 v3 = new float3(vPos.x, vPos.y + 1, 0);
                    float3 v4 = new float3(vPos.x + 1, vPos.y + 1, 0);

                    NativeArray<float2> uvArr = GetUV(position);
                    for (int j = 0; j < 4; j++)
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

        if (tileName != TerrainType.Grass)
        {
            colliders.Add(position);
        }
        else
        {
            oreStates[GetIndex(position)] = GetOreState(position);
        }

        uvArr[0] = uvCoord;
        uvArr[1] = uvCoord + new float2(16f / 80f, 0);
        uvArr[2] = uvCoord + new float2(0, 16f / 48f);
        uvArr[3] = uvCoord + new float2(16f / 80f, 16f / 48f);

        return uvArr;
    }
    public bool GetOreState(float2 position)
    {
        float twoPi = math.PI * 2;

        position += chunkPosition;

        float xVal = math.sin(position.y * twoPi / (float)planetWidth) / twoPi * (float)planetWidth;
        float yVal = math.cos(position.y * twoPi / (float)planetWidth) / twoPi * (float)planetWidth;
        float zVal = math.sin(position.x * twoPi / (float)planetWidth) / twoPi * (float)planetWidth;
        float wVal = math.cos(position.x * twoPi / (float)planetWidth) / twoPi * (float)planetWidth;

        float4 samplePosition = new float4(xVal, yVal, zVal, wVal);
        samplePosition += seed;
        samplePosition /= 50;

        if (noise.cnoise(samplePosition) > 0.6f)
        {
            return true;
        }
        return false;
    }
    private int GetIndex(float2 position)
    {
        return (int)((position.x + 1) * (chunkDimensions.x + 2) + (position.y + 1));
    }
    private float2 GetCoord(TerrainType name)
    {
        float2 uv = new float2(16f / 80f, 16f / 48f);
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

        if (name == TerrainType.CurveTL)
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
    Grass, CornerTL, CornerTR, CornerBL, CornerBR, TopEdge, RightEdge, BottomEdge, LeftEdge, CurveTL, CurveTR, CurveBL, CurveBR
}
