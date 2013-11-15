using UnityEngine;
using System.IO;

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
    private string dirPathToLoad = "";

    // Accessors
    public bool InBatchMode { get{ return inBatchMode; } }

    private BatchModeManager() {
        HandleCommandLineArgs();
        GameManager.LoadDirectory(dirPathToLoad);
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
                default:
                    Debug.Log("Unknown command line arg: " + cmdLnArgs[i]);
                    break;
            }
        }
    }
}
