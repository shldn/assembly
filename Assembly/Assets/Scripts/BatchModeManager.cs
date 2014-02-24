using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// Usage: assembly.exe -batchmode -environment filename -numtests int -keeppercent float -numgenerations int
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
    public int numTests = 1;               // number of tests with one environment
    public int numGenerations = 1;         // number of generations to run.
    public float keepPercent = 1.0f;       // max percentage of tests to keep that are successful in the environment and will mutate
    public float maxFitnessToKeep = 1.0f;  // fitness an assembly must have to persist
    public string envPath = "";

    // Accessors
    public bool InBatchMode { get { return inBatchMode; } }

    private BatchModeManager()
    {
        HandleCommandLineArgs();
        if (inBatchMode)
            RunTest();
    }

    private void RunTest()
    {
        SimulationManager.Inst.Run();
        IOHelper.SaveAllToFolder("./data/" + DateTime.Now.ToString("MMddyyHHmmss") + "/");
        Application.Quit();
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
                    SimulationManager.Inst.envPath = cmdLnArgs[++i];
                    break;
                case "-numtests":
                    SimulationManager.Inst.numTests = int.Parse(cmdLnArgs[++i]);
                    break;
                case "-numGenerations":
                case "-numGen":
                case "-gen":
                    SimulationManager.Inst.numGenerations = int.Parse(cmdLnArgs[++i]);
                    break;
                case "-keepPercent":
                case "-percent":
                    SimulationManager.Inst.keepPercent = float.Parse(cmdLnArgs[++i]);
                    break;
                case "-maxFitness":
                case "-maxFit":
                    SimulationManager.Inst.maxFitnessToKeep = float.Parse(cmdLnArgs[++i]);
                    break;
                case "-mutation":
                case "-mutationDev":
                    SimulationManager.Inst.mutationDeviation = float.Parse(cmdLnArgs[++i]);
                    break;
                case "assemblySize":
                case "numNodes":
                    Debug.LogError("assemblySize not hooked up yet");
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