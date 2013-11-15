﻿using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

public class BatchModeManager {

    private static BatchModeManager mInstance = null;
    public static BatchModeManager Inst {
        get {
            if (mInstance == null)
                mInstance = new BatchModeManager();
            return mInstance;
        }
    }

    // Member variables
    private bool inBatchMode = false;
    private bool mutateAll = false;
    private string dirPathToLoad = "";

    // Accessors
    public bool InBatchMode { get{ return inBatchMode; } }

    private BatchModeManager() {
        HandleCommandLineArgs();
        if (inBatchMode) {
            IOHelper.LoadDirectory(dirPathToLoad);
            if (mutateAll)
                GameManager.MutateAll();
            IOHelper.SaveAllToFolder(IncrementPathName(dirPathToLoad));
            Application.Quit();
        }
    }

    private void HandleCommandLineArgs() {
        string[] cmdLnArgs = System.Environment.GetCommandLineArgs();
        for (int i = 1; i < cmdLnArgs.Length; i++) { // skip exe name
            switch (cmdLnArgs[i]) {
                case "-batchmode":
                    inBatchMode = true;
                    break;
                case "-load":
                case "-path":
                    dirPathToLoad = (cmdLnArgs.Length > i+1) ? cmdLnArgs[i+1] : "";
                    break;
                case "-mutate":
                    mutateAll = true;
                    break;
                default:
                    Debug.Log("Unknown command line arg: " + cmdLnArgs[i]);
                    break;
            }
        }
    }


    // this looks for any number in the string, increments it and replaces it.
    private string IncrementPathName(string nameToIncrement) {
        Match match = Regex.Match(nameToIncrement, @"\d+");
        Debug.LogError("IncrementPathName: " + match.Value + " idx: " + match.Index);
        int num = -1;
        int.TryParse(match.Value, out num);
        return nameToIncrement.Substring(0, match.Index) + (num + 1).ToString() + nameToIncrement.Substring(match.Index + match.Length);
    }
}
