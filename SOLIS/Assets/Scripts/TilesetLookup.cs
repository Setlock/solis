using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TilesetLookup
{
    TextAsset positions, triangulation;
    int tileWidth, tileHeight;
    Dictionary<string, Vector2Int> posDictionary = new Dictionary<string, Vector2Int>();
    Dictionary<string, string> triangulationDictionary = new Dictionary<string, string>();
    public TilesetLookup(TextAsset positions, TextAsset triangulation)
    {
        this.positions = positions;
        this.triangulation = triangulation;
        CreateDictionaries();
    }
    private void CreateDictionaries()
    {
        StringReader reader = new StringReader(positions.text);
        String firstline = reader.ReadLine();
        int commaPos = firstline.IndexOf(",");
        tileWidth = int.Parse(firstline.Substring(0, commaPos));
        tileHeight = int.Parse(firstline.Substring(commaPos + 1));
        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine();
            if (line.Length > 1)
            {
                int equalsPos = line.IndexOf("=");
                int startBracket = line.IndexOf("{");
                int comma = line.IndexOf(",");
                int endBracket = line.IndexOf("}");
                string name = line.Substring(0, equalsPos);
                string xString = line.Substring(startBracket + 1, (comma - startBracket)-1);
                string yString = line.Substring(comma+1, (endBracket-comma)-1);
                int xPos = int.Parse(xString);
                int yPos = int.Parse(yString);
                Vector2Int pos = new Vector2Int(xPos*tileWidth, yPos*tileHeight);
                posDictionary.Add(name, pos);
            }
        }
        reader = new StringReader(triangulation.text);
        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine();
            if (line.Length > 1)
            {
                int equalsPos = line.IndexOf("=");
                string binary = line.Substring(0, equalsPos);
                string name = line.Substring(equalsPos + 1);
                triangulationDictionary.Add(binary, name);
            }
        }
    }
    public Vector2Int GetPosition(string name)
    {
        Vector2Int output = new Vector2Int(0, 0);
        if (posDictionary.ContainsKey(name))
        {
            output = posDictionary[name];
        }
        return output;
    }
    public int getTileWidth()
    {
        return this.tileWidth;
    }
    public int getTileHeight()
    {
        return this.tileHeight;
    }
    public string GetName(string binary)
    {
        string output = "Grass";
        if (triangulationDictionary.ContainsKey(binary))
        {
            output = triangulationDictionary[binary];
        }
        return output;
    }
}
