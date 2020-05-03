using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultLoader : MonoBehaviour
{
    public int chunkWidth = 10, chunkHeight = 10, tileWidth = 1, tileHeight = 1;
    public Dictionary<Vector2, TileData> defaultTileData = new Dictionary<Vector2, TileData>();
    [HideInInspector]
    public Vector3[] defaultVertices;
    [HideInInspector]
    public Vector2[] defaultUV;
    [HideInInspector]
    public Color[] defaultColor;
    public void Start()
    {
        CreateDefaultVertices();
        CreateDefaultTileData();
        SetTileAdjacents();
    }
    public void CreateDefaultTileData()
    {
        int vi = 0;
        for(int j = -1; j <= chunkHeight; j++)
        {
            for(int i = -1; i <= chunkWidth; i++)
            {
                if(i == -1 || j == -1 || i == chunkWidth || j == chunkHeight)
                {
                    defaultTileData.Add(new Vector2(i, j), new TileData(-1, true));
                }
                else
                {
                    defaultTileData.Add(new Vector2(i, j), new TileData(vi, true));
                    vi += 4;
                }
            }
        }
    }
    private void SetTileAdjacents()
    {
        for (int x = 0; x < chunkWidth; x++)
        {
            for (int y = 0; y < chunkHeight; y++)
            {
                Vector2 key = new Vector2(x, y);
                TileData tile = defaultTileData[key];
                TileData tileT = GetTileData(new Vector2(key.x, key.y + 1));
                TileData tileTR = GetTileData(new Vector2(key.x + 1, key.y + 1));
                TileData tileTL = GetTileData(new Vector2(key.x - 1, key.y + 1));

                TileData tileB = GetTileData(new Vector2(key.x, key.y - 1));
                TileData tileBR = GetTileData(new Vector2(key.x + 1, key.y - 1));
                TileData tileBL = GetTileData(new Vector2(key.x - 1, key.y - 1));

                TileData tileL = GetTileData(new Vector2(key.x - 1, key.y));
                TileData tileR = GetTileData(new Vector2(key.x + 1, key.y));

                tile.adjTiles[0] = tileTL;
                tile.adjTiles[1] = tileT;
                tile.adjTiles[2] = tileTR;

                tile.adjTiles[3] = tileL;
                tile.adjTiles[4] = tileR;

                tile.adjTiles[5] = tileBL;
                tile.adjTiles[6] = tileB;
                tile.adjTiles[7] = tileBR;
            }
        }
    }
    private TileData GetTileData(Vector2 position)
    {
        TileData tileData = null;
        if (defaultTileData.ContainsKey(position))
        {
            tileData = defaultTileData[position];
        }
        return tileData;
    }
    public void CreateDefaultVertices()
    {
        defaultVertices = new Vector3[chunkWidth * chunkHeight * 4];
        defaultUV = new Vector2[defaultVertices.Length];
        defaultColor = new Color[defaultVertices.Length];

        int viewChunkWidth = chunkWidth * tileWidth;
        int viewChunkHeight = chunkHeight * tileHeight;
        for (int i = 0, y = 0; y < viewChunkHeight; y += tileWidth)
        {
            for (int x = 0; x < viewChunkWidth; x += tileHeight)
            {
                //Bottom Left
                defaultVertices[i] = new Vector3(x, y);

                //Bottom Right
                defaultVertices[i + 1] = new Vector3(x + tileWidth, y);

                //Top Left
                defaultVertices[i + 2] = new Vector3(x, y + tileHeight);

                //Top Right
                defaultVertices[i + 3] = new Vector3(x + tileWidth, y + tileHeight);

                //Increment index
                i += 4;
            }
        }
    }
}
