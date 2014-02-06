using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class IOHelper
{
    private static int assemblyFileFormatVersion = 1;

    // if the file already exists, make a new one with an incremented file name
    public static string GetValidFileName(string folderPath, string desiredFilename, string ext)
    {
        string filename = folderPath + desiredFilename + ext;
        int count = 1;
        while (File.Exists(filename))
        {
            string tempFileName = string.Format("{0}{1}", folderPath + desiredFilename, count++);
            filename = tempFileName + ext;
        }
        return filename;
    }

    public static void SaveAllToFolder(string folderPath)
    {
        string ext = ".txt";
        Directory.CreateDirectory(folderPath);
        foreach (Assembly a in Assembly.allAssemblies)
            a.Save(GetValidFileName(folderPath, a.name, ext));
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
            name = sr.ReadLine();
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

    static public List<Vector3> Vector3ListFromString(string str)
    {
        List<Vector3> vList = new List<Vector3>();
        string[] tok = str.Split(',');
        if (tok.Length % 3 != 0)
        {
            Debug.LogError("Invalid vector3 list string, tokens should be divisible by 3 != " + tok.Length);
            return null;
        }

        for (int i = 0; i < tok.Length; i += 3)
            vList.Add(new Vector3(float.Parse(tok[i]), float.Parse(tok[i + 1]), float.Parse(tok[i + 2])));
        return vList;
    }

    static public Vector3 Vector3FromString(string str)
    {
        string[] tok = str.Substring(1, str.Length - 2).Split(',');
        float x = float.Parse(tok[0]);
        float y = float.Parse(tok[1]);
        float z = float.Parse(tok[2]);
        return new Vector3(x, y, z);
    }

    static public string ToCommaString(Vector3 v)
    {
        return v.x + "," + v.y + "," + v.z;
    }
}