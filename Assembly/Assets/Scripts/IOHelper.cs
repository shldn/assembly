using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class IOHelper
{
    private static int assemblyFileFormatVersion = 1;

    public static void SaveAllToFolder(string folderPath)
    {
        string ext = ".txt";
        Directory.CreateDirectory(folderPath);
        foreach (Assembly a in Assembly.allAssemblies)
        {
            string filename = folderPath + a.name + ext;
            a.Save(filename);
        }
    }


    public static void LoadDirectory(string dir)
    {
        if (dir == "")
            return;
        try
        {
            Debug.Log("Loading directory " + dir);
            string[] filePaths = Directory.GetFiles(dir);
            foreach (string file in filePaths)
                new Assembly(file);
        }
        catch (Exception e)
        {
            Debug.LogError("LoadDirectory failed: " + e.ToString());
        }
    } // End of LoadDirectory

    public static void LoadAssembly(string filePath, ref string name, ref Vector3 position, ref List<Node> nodes)
    {
        using (StreamReader sr = new StreamReader(filePath))
        {
            int fileFormat = int.Parse(sr.ReadLine());
            name = sr.ReadLine().TrimEnd();
            position = Vector3FromString(sr.ReadLine());
            while (sr.Peek() >= 0)
                nodes.Add(Node.FromString(sr.ReadLine()));
        }
    }

    public static void SaveAssembly(string filePath, Assembly assembly)
    {
        if (!File.Exists(filePath))
        {
            // Create a file to write to. 
            using (StreamWriter sw = File.CreateText(filePath))
            {
                sw.WriteLine(assemblyFileFormatVersion);
                sw.WriteLine(assembly.name);
                sw.WriteLine(assembly.worldPosition);
                for (int i = 0; i < assembly.nodes.Count; ++i)
                    sw.WriteLine(assembly.nodes[i].ToFileString(assemblyFileFormatVersion));
            }
        }
        else
            Debug.LogError(filePath + " already exists, save aborted");
    }

    static public Vector3 Vector3FromString(string str)
    {
        string[] temp = str.Substring(1, str.Length - 2).Split(',');
        float x = float.Parse(temp[0]);
        float y = float.Parse(temp[1]);
        float z = float.Parse(temp[2]);
        return new Vector3(x, y, z);
    }

    static public IntVector3 IntVector3FromString(string str)
    {
        string[] temp = str.Substring(1, str.Length - 2).Split(',');
        int x = int.Parse(temp[0]);
        int y = int.Parse(temp[1]);
        int z = int.Parse(temp[2]);
        return new IntVector3(x, y, z);
    }
}