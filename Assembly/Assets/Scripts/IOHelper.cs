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
        foreach (PhysAssembly a in PhysAssembly.getAll)
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
                new PhysAssembly(file, null, null, true);
        }
        catch (Exception e)
        {
            Debug.LogError("LoadDirectory failed: " + e.ToString());
        }
    } // End of LoadDirectory

    public static void LoadAssemblyFromFile(string filePath, ref string name, ref Vector3 position, ref List<PhysNode> nodes)
    {
        using (StreamReader sr = new StreamReader(filePath))
        {
            ReadAssemblyFromStream(sr, ref name, ref position, ref nodes);
        }
    }

    public static void LoadAssemblyFromString(string assemblyStr, ref string name, ref Vector3 position, ref List<PhysNode> nodes)
    {
        StringReader sr = new StringReader(assemblyStr);
        ReadAssemblyFromStream(sr, ref name, ref position, ref nodes);
    }

    public static string AssemblyToString(PhysAssembly assembly){
        StringWriter sw = new StringWriter();
        WriteAssemblyToStream(assembly, sw);
        return sw.ToString();
    } // End of AssemblyToString().

    private static void ReadAssemblyFromStream(TextReader stream, ref string name, ref Vector3 position, ref List<PhysNode> nodes)
    {
        int fileFormat = int.Parse(stream.ReadLine());
        name = stream.ReadLine();
        position = Vector3FromString(stream.ReadLine());
        while (stream.Peek() >= 0)
            nodes.Add(PhysNode.FromString(stream.ReadLine()));
    }

    private static void WriteAssemblyToStream(PhysAssembly assembly, TextWriter stream){
        stream.WriteLine(assemblyFileFormatVersion);
        stream.WriteLine(assembly.name);
        stream.WriteLine(assembly.Position);
        foreach(PhysNode someNode in assembly.NodeDict.Values)
            stream.WriteLine(someNode.ToFileString(assemblyFileFormatVersion));
    } // End of WriteAssemblyToStream().

    public static void SaveAssembly(string filePath, PhysAssembly assembly)
    {
#if UNITY_STANDALONE
        if (!File.Exists(filePath))
        {
            // Create a file to write to. 
            using (StreamWriter sw = File.CreateText(filePath))
            {
                WriteAssemblyToStream(assembly, sw);
            }
        }
        else
            Debug.LogError(filePath + " already exists, save aborted");
#else
        Debug.LogError("Save Assembly only supported in standalone builds.");
#endif
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

    static public Quaternion QuaternionFromString(string str)
    {
        string[] tok = str.Substring(1, str.Length - 2).Split(',');
        float x = float.Parse(tok[0]);
        float y = float.Parse(tok[1]);
        float z = float.Parse(tok[2]);
        float w = float.Parse(tok[3]);
        return new Quaternion(x, y, z, w);
    }

    static public Triplet TripletFromString(string str)
    {
        string[] tok = str.Substring(1, str.Length - 2).Split(',');
        int x = int.Parse(tok[0]);
        int y = int.Parse(tok[1]);
        int z = int.Parse(tok[2]);
        return new Triplet(x, y, z);
    }
    static public string ToCommaString(Vector3 v)
    {
        return v.x + "," + v.y + "," + v.z;
    }



	// Overloads for PhysAssembly assets ----------------------------------------------------------------- //

    

	

}