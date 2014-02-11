using UnityEngine;
using System;
using System.Collections.Generic;

public class SimulationManager : MonoBehaviour {

    public int numTests = 20;               // number of tests with one environment
    public int numGenerations = 1;         // number of generations to run.
    public float keepPercent = 1.0f;       // max percentage of tests to keep that are successful in the environment and will mutate
    public float maxFitnessToKeep = 45.0f;  // fitness an assembly must have to persist
    public float mutationDeviation = 0.01f; // amount of mutation for each test on the secondary generations
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

    private int CompareFitness(KeyValuePair<float, Assembly> lhs, KeyValuePair<float, Assembly> rhs)
    {
        return lhs.Key.CompareTo(rhs.Key);
    }

    public void Run()
    {
        DateTime startTime = DateTime.Now;
        int maxNumToKeep = (int)(keepPercent * (float)numTests);

        // setup environment
        EnvironmentManager.Load(envPath);
        EnvironmentManager.InitializeFood();

        // first pass -- random assemblies
        List<KeyValuePair<float, Assembly>> assemblyScores = new List<KeyValuePair<float, Assembly>>();
        RunEnvironmentTest(null, ref assemblyScores);
        KeepTheBest(maxNumToKeep, ref assemblyScores);
        if (assemblyScores.Count == 0)
        {
            Debug.Log("No assemblies fit the criteria, try again");
            return;
        }
        Debug.Log("Found " + assemblyScores.Count + " successful random assemblies, mutating...");

        // second pass -- mutations on successful assemblies
        for(int i=0; i < numGenerations; ++i)
        {
            int lastGenerationCount = assemblyScores.Count;
            for (int aIdx = 0; aIdx < lastGenerationCount; ++aIdx)
                RunEnvironmentTest(assemblyScores[aIdx].Value, ref assemblyScores);
            KeepTheBest(maxNumToKeep, ref assemblyScores);

            float bestFitness = (assemblyScores.Count > 0) ? assemblyScores[0].Key : -1;
            float worstFitness = (assemblyScores.Count > 0) ? assemblyScores[assemblyScores.Count-1].Key : -1;
            Debug.Log("Generation " + (i + 1) + " complete, " + assemblyScores.Count + " successful assemblies, best fitness: " + bestFitness + " worst: " + worstFitness);

            // optimization, don't keep ones we are going to throw away
            if (assemblyScores.Count == maxNumToKeep)
                maxFitnessToKeep = worstFitness;
        }

        Debug.Log(numGenerations + " Generations ran in " + DateTime.Now.Subtract(startTime).ToString());
    }

    void RunEnvironmentTest(Assembly seed, ref List<KeyValuePair<float, Assembly>> assemblyScores)
    {
        for (int testCount = 0; testCount < numTests; ++testCount)
        {
            List<Assembly> assembliesToTest = EnvironmentManager.InitializeAssemblies(seed, mutationDeviation);
            for (int i = 0; i < assembliesToTest.Count; ++i)
            {
                float fitness = assembliesToTest[i].Fitness();
                if (fitness < maxFitnessToKeep)
                    assemblyScores.Add(new KeyValuePair<float, Assembly>(fitness, assembliesToTest[i]));
                else
                    assembliesToTest[i].Destroy();
            }
        }
    }

    void KeepTheBest(int numToKeep, ref List<KeyValuePair<float, Assembly>> assemblyScores)
    {
        assemblyScores.Sort(CompareFitness);
        for (int i = assemblyScores.Count - 1; i >= numToKeep; --i)
        {
            assemblyScores[i].Value.Destroy();
            assemblyScores.RemoveAt(i);
        }
    }

}
