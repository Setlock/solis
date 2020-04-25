using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.UIElements;
using UnityEngine;

public class TilesetLookup
{
    string posFilename = "";
    string triangulationFilename = "";
    Dictionary<string, Vector2> posDictionary = new Dictionary<string, Vector2>();
    Dictionary<string, string> triangulationDictionary = new Dictionary<string, string>();
    public TilesetLookup(string posFilename, string triangulationFilename)
    {
        this.posFilename = posFilename;
        this.triangulationFilename = triangulationFilename;
        //CreateDictionaries();
    }
    private void CreateDictionaries()
    {
        StreamReader reader = new StreamReader(posFilename);
        while (!reader.EndOfStream)
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
                Vector2 pos = new Vector2(xPos, yPos);
                posDictionary.Add(name, pos);
            }
        }
        reader = new StreamReader(triangulationFilename);
        while (!reader.EndOfStream)
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
    public Vector2 GetPosition(string name)
    {
        Vector2 output = new Vector2(0, 0);
        if (posDictionary.ContainsKey(name))
        {
            output = posDictionary[name];
        }
        return output;
    }
    public string GetName(string binary)
    {
        string output = "Grass";
        if (triangulationDictionary.ContainsKey(binary))
        {
            output = triangulationDictionary[binary];
        }
        return output;
        /*foreach(string key in triangulationDictionary.Keys)
        {
            bool found = true;
            for(int i = 0; i < key.Length; i++)
            {
                if (!key.Substring(i, 1).Equals("2"))
                {
                    if(!key.Substring(i, 1).Equals(binary.Substring(i, 1)))
                    {
                        found = false;
                    }
                }
            }
            if (found)
            {
                output = triangulationDictionary[key];
            }
        }
        return output;*/
    }
}
