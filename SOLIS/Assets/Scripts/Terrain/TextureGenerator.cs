using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class TextureGenerator : MonoBehaviour
{
    public Material refMaterial;
    public GameObject spherePosition;
    public GameObject prefab;

    GameObject prefabObject;

    public Color landColor, waterColor;
    public int numLayers;
    public float noiseCutoff, featureSize, recede, baseRoughness, roughness, persistence, strength;
    // Start is called before the first frame update
    void Start()
    {
        /*prefabObject = Instantiate(prefab);
        prefabObject.transform.position = spherePosition.transform.position;
        GenerateCubeMap();*/
    }
    public void Update()
    {
        
    }
    public void OnValidate()
    {
        if (Application.isPlaying)
        {
            if (GUI.changed)
            {
                GenerateCubeMap();
            }
        }
    }
    public float GetNoise(float3 samplePos)
    {
        samplePos /= featureSize;
        float noiseValue = 0;
        float frequency = baseRoughness;
        float amplitude = 1;

        for (int i = 0; i < numLayers; i++)
        {
            float v = noise.snoise(samplePos * frequency);
            noiseValue += (v + 1) * 0.5f * amplitude;
            frequency *= roughness;
            amplitude *= persistence;
        }

        float finalValue = Mathf.Max(0, noiseValue - recede);

        return finalValue * strength;
    }
    public Color GetColor(float value)
    {
        if(value < noiseCutoff)
        {
            return landColor;
        }
        return waterColor;
    }
    public void GenerateCubeMap()
    {
        int dim = 100;
        Texture2D finalMap = new Texture2D(dim*4,dim*3);

		//Z STATIC
		for (int y = 0; y < dim; y++)
		{
			for (int x = 0; x < dim * 2; x++)
			{
				//Generates FRONT
				if (x < dim)
				{
					int2 pixelPos = new int2(dim + x, dim + y);
					float3 samplePos = new float3(x, y, 0);
					float noiseVal = GetNoise(samplePos);
                    finalMap.SetPixel(pixelPos.x, pixelPos.y, GetColor(noiseVal));
				}
				//Generates BACK
				else
				{
					int2 pixelPos = new int2(dim*3 + (x-dim), dim + y);
					float3 samplePos = new float3(dim - (x - dim), y, dim);
                    float noiseVal = GetNoise(samplePos);
					finalMap.SetPixel(pixelPos.x, pixelPos.y, GetColor(noiseVal));
				}
			}
		}
		//X STATIC
		for (int y = 0; y < dim; y++)
		{
			for (int x = 0; x < dim * 2; x++)
			{
				//Generates LEFT
				if (x < dim)
				{
					int2 pixelPos = new int2(x, dim + y);
					float3 samplePos = new float3(0, y, dim - x);
					float noiseVal = GetNoise(samplePos);
                    finalMap.SetPixel(pixelPos.x, pixelPos.y, GetColor(noiseVal));
				}
				//Generates RIGHT
				else
				{
					int2 pixelPos = new int2(dim*2 + (x-dim), dim + y);
					float3 samplePos = new float3(dim, y, x - dim);
					float noiseVal = GetNoise(samplePos);
                    finalMap.SetPixel(pixelPos.x, pixelPos.y, GetColor(noiseVal));
				}
			}
		}
		//Y STATIC
		for (int y = 0; y < dim * 2; y++)
		{
			for (int x = 0; x < dim; x++)
			{
				//Generates TOP
				if (y < dim)
				{
					int2 pixelPos = new int2(dim + x, y);
					float3 samplePos = new float3(x, 0, dim - y);
					float noiseVal = GetNoise(samplePos);
                    finalMap.SetPixel(pixelPos.x, pixelPos.y, GetColor(noiseVal));
				}
				//Generates BOTTOM
				else
				{
					int2 pixelPos = new int2(dim + x, dim*2 + (y-dim));
					float3 samplePos = new float3(x, dim, y - dim);
					float noiseVal = GetNoise(samplePos);
                    finalMap.SetPixel(pixelPos.x, pixelPos.y, GetColor(noiseVal));
				}
			}
		}
		finalMap.Apply();

        Material newMat = new Material(refMaterial);
        newMat.mainTexture = ConvertToEquirectangular(finalMap, 1000, 1000);
        prefabObject.GetComponent<MeshRenderer>().material = newMat;
    }
    public static Texture2D ConvertToEquirectangular(Texture2D sourceTexture, int outputWidth, int outputHeight)
    {
        Texture2D equiTexture = new Texture2D(outputWidth, outputHeight, TextureFormat.ARGB32, false);
        float u, v; //Normalised texture coordinates, from 0 to 1, starting at lower left corner
        float phi, theta; //Polar coordinates
        int cubeFaceWidth, cubeFaceHeight;

        cubeFaceWidth = sourceTexture.width / 4; //4 horizontal faces
        cubeFaceHeight = sourceTexture.height / 3; //3 vertical faces


        for (int j = 0; j < equiTexture.height; j++)
        {
            //Rows start from the bottom
            v = 1 - ((float)j / equiTexture.height);
            theta = v * Mathf.PI;

            for (int i = 0; i < equiTexture.width; i++)
            {
                //Columns start from the left
                u = ((float)i / equiTexture.width);
                phi = u * 2 * Mathf.PI;

                float x, y, z; //Unit vector
                x = Mathf.Sin(phi) * Mathf.Sin(theta) * -1;
                y = Mathf.Cos(theta);
                z = Mathf.Cos(phi) * Mathf.Sin(theta) * -1;

                float xa, ya, za;
                float a;

                a = Mathf.Max(new float[3] { Mathf.Abs(x), Mathf.Abs(y), Mathf.Abs(z) });

                //Vector Parallel to the unit vector that lies on one of the cube faces
                xa = x / a;
                ya = y / a;
                za = z / a;

                Color color;
                int xPixel, yPixel;
                int xOffset, yOffset;

                if (xa == 1)
                {
                    //Right
                    xPixel = (int)((((za + 1f) / 2f) - 1f) * cubeFaceWidth);
                    xOffset = 2 * cubeFaceWidth; //Offset
                    yPixel = (int)((((ya + 1f) / 2f)) * cubeFaceHeight);
                    yOffset = cubeFaceHeight; //Offset
                }
                else if (xa == -1)
                {
                    //Left
                    xPixel = (int)((((za + 1f) / 2f)) * cubeFaceWidth);
                    xOffset = 0;
                    yPixel = (int)((((ya + 1f) / 2f)) * cubeFaceHeight);
                    yOffset = cubeFaceHeight;
                }
                else if (ya == 1)
                {
                    //Up
                    xPixel = (int)((((xa + 1f) / 2f)) * cubeFaceWidth);
                    xOffset = cubeFaceWidth;
                    yPixel = (int)((((za + 1f) / 2f) - 1f) * cubeFaceHeight);
                    yOffset = 2 * cubeFaceHeight;
                }
                else if (ya == -1)
                {
                    //Down
                    xPixel = (int)((((xa + 1f) / 2f)) * cubeFaceWidth);
                    xOffset = cubeFaceWidth;
                    yPixel = (int)((((za + 1f) / 2f)) * cubeFaceHeight);
                    yOffset = 0;
                }
                else if (za == 1)
                {
                    //Front
                    xPixel = (int)((((xa + 1f) / 2f)) * cubeFaceWidth);
                    xOffset = cubeFaceWidth;
                    yPixel = (int)((((ya + 1f) / 2f)) * cubeFaceHeight);
                    yOffset = cubeFaceHeight;
                }
                else if (za == -1)
                {
                    //Back
                    xPixel = (int)((((xa + 1f) / 2f) - 1f) * cubeFaceWidth);
                    xOffset = 3 * cubeFaceWidth;
                    yPixel = (int)((((ya + 1f) / 2f)) * cubeFaceHeight);
                    yOffset = cubeFaceHeight;
                }
                else
                {
                    Debug.LogWarning("Unknown face, something went wrong");
                    xPixel = 0;
                    yPixel = 0;
                    xOffset = 0;
                    yOffset = 0;
                }

                xPixel = Mathf.Abs(xPixel);
                yPixel = Mathf.Abs(yPixel);

                xPixel += xOffset;
                yPixel += yOffset;

                color = sourceTexture.GetPixel(xPixel, yPixel);
                equiTexture.SetPixel(i, j, color);
            }
        }

        equiTexture.Apply();
        /*var bytes = equiTexture.EncodeToPNG();
        Object.DestroyImmediate(equiTexture);
        */
        return equiTexture;
    }
}
