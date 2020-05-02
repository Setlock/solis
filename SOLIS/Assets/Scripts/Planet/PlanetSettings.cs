using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class PlanetSettings : ScriptableObject
{
    public long planetSeed;
    public float planetFeatureSize;
    public Color terrainColor;
    public Color waterColor;
}
