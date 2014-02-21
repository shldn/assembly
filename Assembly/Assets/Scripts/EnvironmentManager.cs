using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class EnvironmentManager {

    private static int envFileFormatVersion = 2;
    private static int envFilePositionOnlyFormatVersion = 1;
    public static List<Vector3> foodPositions;
    public static List<Vector3> assemblyPositions;
    public static int assemblyMinSize = 5;
    public static int assemblyMaxSize = 15;

    public static void Load(string file)
    {
        if (file == null || file == "")
        {
            Debug.LogError("EnvironmentManager null file name");
            return;
        }

        using (StreamReader sr = new StreamReader(file))
        {
            int fileFormat = int.Parse(sr.ReadLine());
            string id = sr.ReadLine();
            foodPositions = IOHelper.Vector3ListFromString(sr.ReadLine());

            if (fileFormat == envFilePositionOnlyFormatVersion)
                assemblyPositions = IOHelper.Vector3ListFromString(sr.ReadLine());
            else if (fileFormat == envFileFormatVersion)
                IOHelper.LoadDirectory(sr.ReadLine());
            else
                Debug.LogError("Unsupported environment file version: " + fileFormat);
        }

        InitializeFood();
    }

    public static void Save(string file)
    {
        if (!File.Exists(file) && !string.IsNullOrEmpty(file))
        {
            string nowStr = DateTime.Now.ToString("MMddyyHHmmssff");
            string path = file.Substring(0,file.LastIndexOfAny("/\\".ToCharArray())+1);
            string aDir = path + nowStr + "/";

            // Create a file to write to. 
            using (StreamWriter sw = File.CreateText(file))
            {
                sw.WriteLine(envFileFormatVersion);
                sw.WriteLine(nowStr);

                // write all food node positions
                WriteFoodPositions(sw);

                // write path to directory with all assemblies  
                sw.WriteLine(aDir);
            }
            IOHelper.SaveAllToFolder(aDir);
        }
        else
            Debug.LogError(file + " already exists, save aborted");
    }

    public static void SavePositionsOnly(string file)
    {
        if (!File.Exists(file))
        {
            // Create a file to write to. 
            using (StreamWriter sw = File.CreateText(file))
            {
                sw.WriteLine(envFilePositionOnlyFormatVersion);
                sw.WriteLine(DateTime.Now.ToString("MMddyyHHmmssff"));

                // write all food node positions
                WriteFoodPositions(sw);

                // write all assembly positions
                string aStr = (Assembly.GetAll().Count > 0) ? IOHelper.ToCommaString(Assembly.GetAll()[0].WorldPosition) : "";
                for (int i = 1; i < Assembly.GetAll().Count; ++i)
                    aStr += "," + IOHelper.ToCommaString(Assembly.GetAll()[i].WorldPosition);
                sw.WriteLine(aStr);
            }
        }
        else
            Debug.LogError(file + " already exists, save aborted");
    }

    // If seed is supplied, places mutated assemblies of the seed assembly at each assembly position in the assemblyPositions list
    // If seed == null, places random assemblies at each assembly position in the assemblyPositions list
    public static List<Assembly> InitializeAssemblies(Assembly seed, float mutationDeviation = 0.01f)
    {
        List<Assembly> newAssemblies = new List<Assembly>();
        for (int i = 0; assemblyPositions != null && i < assemblyPositions.Count; ++i)
        {
            Assembly a = null;
            if( seed == null )
                 a = Assembly.GetRandomAssembly(UnityEngine.Random.Range(assemblyMinSize, assemblyMaxSize));
            else
            {
                a = seed.Duplicate();
                a.Mutate(mutationDeviation);
            }
            a.WorldPosition = assemblyPositions[i];
            newAssemblies.Add(a);
        }
        return newAssemblies;
    }

    private static void InitializeFood()
    {
        for (int i = 0; foodPositions != null && i < foodPositions.Count; ++i)
            new FoodPellet(foodPositions[i]);
    }

    private static void WriteFoodPositions(StreamWriter sw)
    {
        string foodStr = (FoodPellet.GetAll().Count > 0) ? IOHelper.ToCommaString(FoodPellet.GetAll()[0].worldPosition) : "";
        for (int i = 1; i < FoodPellet.GetAll().Count; ++i)
            foodStr += "," + IOHelper.ToCommaString(FoodPellet.GetAll()[i].worldPosition);
        sw.WriteLine(foodStr);
    }

}
