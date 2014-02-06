#define RUN_AS_MONOBEHAVIOR

using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;


// assembly.exe -batchmode -environment filename -numtests int -keeppercent float -numgenerations int
public class BatchModeManager
#if (RUN_AS_MONOBEHAVIOR)
 : MonoBehaviour
#endif
{

    private static BatchModeManager mInstance = null;
    public static BatchModeManager Inst
    {
        get
        {
            if (mInstance == null)
                mInstance = new BatchModeManager();
            return mInstance;
        }
    }

    // Member variables
    private bool inBatchMode = false;
    public int numTests = 1;               // number of tests with one environment
    public int numGenerations = 1;         // number of generations to run.
    public float keepPercent = 1.0f;       // max percentage of tests to keep that are successful in the environment and will mutate
    public float maxFitnessToKeep = 1.0f;  // fitness an assembly must have to persist
    public string envPath = "";

    // Accessors
    public bool InBatchMode { get { return inBatchMode; } }

#if (RUN_AS_MONOBEHAVIOR)
    void Awake(){
        mInstance = this;
    }
#endif

    private BatchModeManager()
    {
#if (RUN_AS_MONOBEHAVIOR)
        return;
#endif
        HandleCommandLineArgs();
        if (inBatchMode)
            RunTest();
    }

    public void RunTest()
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
        //Application.Quit();
    }

    private void HandleCommandLineArgs()
    {
        string[] cmdLnArgs = System.Environment.GetCommandLineArgs();
        for (int i = 1; i < cmdLnArgs.Length; i++)
        { // skip exe name
            switch (cmdLnArgs[i])
            {
                case "-batchmode":
                    inBatchMode = true;
                    break;

                case "-environment":
                case "-env":
                    envPath = cmdLnArgs[++i];
                    break;
                case "-numtests":
                    numTests = int.Parse(cmdLnArgs[++i]);
                    break;
                case "-numGenerations":
                case "-numGen":
                case "-gen":
                    numGenerations = int.Parse(cmdLnArgs[++i]);
                    break;
                case "-keepPercent":
                case "-percent":
                    keepPercent = float.Parse(cmdLnArgs[++i]);
                    break;
                default:
                    if( inBatchMode )   
                        Debug.Log("Unknown command line arg: " + cmdLnArgs[i]);
                    break;
            }
        }
    }


    // this looks for any number in the string, increments it and replaces it.
    private string IncrementPathName(string nameToIncrement)
    {
        Match match = Regex.Match(nameToIncrement, @"\d+");
        Debug.LogError("IncrementPathName: " + match.Value + " idx: " + match.Index);
        int num = -1;
        int.TryParse(match.Value, out num);
        return nameToIncrement.Substring(0, match.Index) + (num + 1).ToString() + nameToIncrement.Substring(match.Index + match.Length);
    }
}