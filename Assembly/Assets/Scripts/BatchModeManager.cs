using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// assembly.exe -batchmode -environment filename -numtests int -keeppercent float -numgenerations int
public class BatchModeManager
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
    private int numTests = 1;           // number of tests with one environment
    private int numGenerations = 1;     // number of generations to run.
    private float keepPercent = 1.0f;   // max percentage of tests to keep that are successful in the environment and will mutate
    private string envPath = "";

    // Accessors
    public bool InBatchMode { get { return inBatchMode; } }

    private BatchModeManager()
    {
        HandleCommandLineArgs();
        if (inBatchMode)
        {
            DateTime startTime = DateTime.Now;
            EnvironmentManager.Load(envPath);
            SortedDictionary<float, Assembly> assemblyScores = new SortedDictionary<float, Assembly>(); // mapping of fitness score --> assembly

            for (int i = 0; i < Assembly.GetAll().Count; ++i)
            {
                assemblyScores.Add(Assembly.GetAll()[i].Fitness(), Assembly.GetAll()[i]); 
            }

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
            System.Console.WriteLine(numGenerations + " Generations ran in " + DateTime.Now.Subtract(startTime).ToString());
            //Application.Quit();
        }
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