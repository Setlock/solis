using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

public class TerrainChunk
{
    TerrainHandler handler;
    public Dictionary<Vector2, Tile> tiles = new Dictionary<Vector2, Tile>();
    public Vector2 worldPosition;
    public Vector2 position;
    public Vector2 dimensions;
    public Vector2 tileSize;

    GameObject chunkObject;

    bool remove = false;
    public TerrainChunk(TerrainHandler handler, GameObject chunkObject, Vector2 position, Vector2 worldPosition, Vector2 dimensions, Vector2 tileSize)
    {
        this.handler = handler;
        this.position = position;
        this.worldPosition = worldPosition;
        this.dimensions = dimensions;
        this.tileSize = tileSize;

        chunkObject.transform.position = worldPosition;
        this.chunkObject = chunkObject;
    }
    NativeArray<bool> states;
    public JobHandle CreateStateJob(int seed, int numLayers, float featureSize, float recede, float baseRoughness, float roughness, float persistence, float strength)
    {
        int jobSize = (int)((dimensions.x + 2) * (dimensions.y + 2));
        states = new NativeArray<bool>(jobSize, Allocator.Persistent);

        int2 chunkPosition = new int2((int)position.x, (int)position.y);
        int2 chunkDimensions = new int2((int)dimensions.x, (int)dimensions.y);
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
            states = states
        };
        return valueJob.Schedule(jobSize, (int)(dimensions.x + 2));
    }
    public void CompleteStateJob(JobHandle job)
    {
        job.Complete();
    }

    NativeList<float3> vertices;
    NativeList<int> triangles;
    NativeList<float2> uvs;
    public JobHandle CreateMeshJob()
    {
        vertices = new NativeList<float3>(Allocator.Persistent);
        triangles = new NativeList<int>(Allocator.Persistent);
        uvs = new NativeList<float2>(Allocator.Persistent);

        int2 chunkDimensions = new int2((int)dimensions.x, (int)dimensions.y);
        int2 tileSize = new int2((int)this.tileSize.x, (int)this.tileSize.y);
        MeshJob meshJob = new MeshJob
        {
            states = states,
            chunkDimensions = chunkDimensions,
            tileSize = tileSize,
            vertices = vertices,
            uvs = uvs,
            triangles = triangles
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
    }
    public GameObject Remove()
    {
        chunkObject.SetActive(false);
        tiles.Clear();
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
