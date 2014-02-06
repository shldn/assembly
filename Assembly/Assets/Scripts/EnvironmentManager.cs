using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class EnvironmentManager {

    private static int envFileFormatVersion = 1;
    public static List<Vector3> foodPositions;
    public static List<Vector3> assemblyPositions;
    public static int assemblySize = 10;

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
            assemblyPositions = IOHelper.Vector3ListFromString(sr.ReadLine());
        }
    }

    public static void Save(string file)
    {
        if (!File.Exists(file))
        {
            // Create a file to write to. 
            using (StreamWriter sw = File.CreateText(file))
            {
                sw.WriteLine(envFileFormatVersion);
                sw.WriteLine(DateTime.UtcNow.ToBinary().ToString());

                // write all food node positions
                string foodStr = (FoodNode.GetAll().Count > 0) ? IOHelper.ToCommaString(FoodNode.GetAll()[0].worldPosition) : "";
                for (int i = 1; i < FoodNode.GetAll().Count; ++i)
                    foodStr += "," + IOHelper.ToCommaString(FoodNode.GetAll()[i].worldPosition);
                sw.WriteLine(foodStr);

                // write all assembly positions
                string aStr = (Assembly.allAssemblies.Count > 0) ? IOHelper.ToCommaString(Assembly.allAssemblies[0].worldPosition) : "";
                for (int i = 1; i < Assembly.allAssemblies.Count; ++i)
                    aStr += "," + IOHelper.ToCommaString(Assembly.allAssemblies[i].worldPosition);
                sw.WriteLine(aStr);
            }
        }
        else
            Debug.LogError(file + " already exists, save aborted");
    }

    public static void InitializeFood()
    {
        for (int i = 0; foodPositions != null && i < foodPositions.Count; ++i)
            new FoodNode(foodPositions[i]);
    }

    // Places random assemblies at each assembly position in the assemblyPositions list
    public static List<Assembly> InitializeAssemblies()
    {
        List<Assembly> newAssemblies = new List<Assembly>();
        for (int i = 0; assemblyPositions != null && i < assemblyPositions.Count; ++i)
        {
            Assembly a = Assembly.GetRandomAssembly(assemblySize);
            a.worldPosition = assemblyPositions[i];
            newAssemblies.Add(a);
        }
        return newAssemblies;
    }

}
