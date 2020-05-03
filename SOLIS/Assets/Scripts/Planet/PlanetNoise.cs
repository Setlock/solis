using System.ComponentModel;
using Unity.Mathematics;
using UnityEngine;

public class PlanetNoise
{
    public static float GetValueAtPosition(float4 pos)
    {
        float noiseValue = 0;

        float frequency = 0.91f;
        float amplitude = 1;
        float persistence = 0.54f;
        float roughness = 1.83f;
        float strength = 1;
        int numLayers = 3;
        float recede = 0.7f;

        for(int i = 0; i < numLayers; i++)
        {
            float v = noise.cnoise(pos*frequency);
            noiseValue += (v+1)*0.5f * amplitude;
            frequency *= roughness;
            amplitude *= persistence;
        }
        noiseValue = Mathf.Max(0, noiseValue - recede);
        return noiseValue * strength;
    }
    public static bool GetStateFromValue(float val)
    {
        bool output = false;
        if(val >= 0.2f)
        {
            output = true;
        }
        return output;
    }
    public static bool GetStateFromPos(float4 pos)
    {
        return GetStateFromValue(GetValueAtPosition(pos));
    }
}
