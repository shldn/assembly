using UnityEngine;
using System;
using System.Collections.Generic;

public class SimulationManager : MonoBehaviour {

    public int numTests = 20;               // number of tests with one environment
    public int numGenerations = 1;         // number of generations to run.
    public float keepPercent = 1.0f;       // max percentage of tests to keep that are successful in the environment and will mutate
    public float maxFitnessToKeep = 45.0f;  // fitness an assembly must have to persist
    public string envPath = "./data/env.txt";             // path to the file that defines the environment, positions of food and assemblies

    private static SimulationManager mInstance = null;
    public static SimulationManager Inst
    {
        get
        {
            if (mInstance == null)
                mInstance = new SimulationManager();
            return mInstance;
        }
    }

    void Awake(){
        mInstance = this;
    }

    public void Run()
    {
        DateTime startTime = DateTime.Now;
        EnvironmentManager.Load(envPath);
        EnvironmentManager.InitializeFood();

        List<KeyValuePair<float, Assembly>> assemblyScores = new List<KeyValuePair<float, Assembly>>();
        for (int testCount = 0; testCount < numTests; ++testCount)
        {
            List<Assembly> assembliesToTest = EnvironmentManager.InitializeAssemblies();
            for (int i = 0; i < assembliesToTest.Count; ++i)
                assemblyScores.Add(new KeyValuePair<float, Assembly>(assembliesToTest[i].Fitness(), assembliesToTest[i]));
        }

        int maxNumToKeep = (int)(keepPercent * (float)Assembly.GetAll().Count);
        List<Assembly> assembliesToKeep = new List<Assembly>();
        foreach (KeyValuePair<float, Assembly> kvp in assemblyScores)
        {
            if (kvp.Key < maxFitnessToKeep && assembliesToKeep.Count < maxNumToKeep)
                assembliesToKeep.Add(kvp.Value);
            else
                kvp.Value.Destroy();
        }

        Debug.LogError("Found " + assembliesToKeep.Count + " assemblies");


        /*
        IOHelper.LoadDirectory(dirPathToLoad);
        string nextPathToLoad = dirPathToLoad;
        for (int i = 0; i < numIterations; ++i)
        {
            if (mutateAll)
                GameManager.MutateAll();
            nextPathToLoad = IncrementPathName(nextPathToLoad);
            IOHelper.SaveAllToFolder(nextPathToLoad);
        }
        */
        Debug.LogError(numGenerations + " Generations ran in " + DateTime.Now.Subtract(startTime).ToString());
    }
}
