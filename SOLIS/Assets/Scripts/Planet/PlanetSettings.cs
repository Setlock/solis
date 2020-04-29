using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class PlanetSettings : ScriptableObject
{
    [Range(1, 50)]
    public int chunkWidth = 10, chunkHeight = 10;
    public int tileWidth = 1, tileHeight = 1;
    public long planetSeed;
    public float planetFeatureSize;
    public Color terrainColor;
    public Color waterColor;
}
