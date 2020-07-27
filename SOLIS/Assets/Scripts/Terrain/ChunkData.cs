using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChunkData
{
    Dictionary<Vector2, GameObject> tiles;
    public ChunkData()
    {
        tiles = new Dictionary<Vector2, GameObject>();
    }
    public void Update()
    {
        Vector2[] keys = tiles.Keys.ToArray();
        foreach(Vector2 key in keys)
        {
            if (tiles[key].GetComponent<Tile>().remove)
            {
                GameObject.Destroy(tiles[key].gameObject);
                tiles.Remove(key);
            }
        }
    }
    public Dictionary<Vector2, GameObject> GetTileDictionary()
    {
        return tiles;
    }
    public void SetTileDictionary(Dictionary<Vector2, GameObject> tiles)
    {
        this.tiles = tiles;
    }
    public void AddTile(Vector2 position, GameObject tile)
    {
        this.tiles.Add(position, tile);
    }
    public void SetTile(Vector2 position, GameObject tile)
    {
        this.tiles[position] = tile;
    }
    public GameObject GetTile(Vector2 position)
    {
        return tiles[position];
    }
}
