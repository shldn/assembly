using UnityEngine;

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

    // Accessors
    public bool InBatchMode { get{ return InBatchMode; } }


    private BatchModeManager() {
        HandleCommandLineArgs();
    }

    private void HandleCommandLineArgs() {
        string[] cmdLnArgs = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < cmdLnArgs.Length; i++) {
            switch (cmdLnArgs[i]) {
                case "-batchmode":
                    inBatchMode = true;
                    break;
                default:
                    Debug.LogError("Unknown command line arg: " + cmdLnArgs[i]);
                    break;
            }
        }
    }
}
