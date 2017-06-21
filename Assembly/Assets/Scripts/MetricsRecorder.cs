using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class MetricsRecorder : MonoBehaviour {
    private float startTime;
    private static string fileName;
    private static string sessionID;
    private static bool graphics;
    private static string compositionHeader;
    
    private static MetricsRecorder mInst = null;
    public static MetricsRecorder Inst
    {
        get
        {
            if (mInst == null)
                mInst = (new GameObject("MetricsRecorder")).AddComponent<MetricsRecorder>();
            return mInst;
        }
    }

    void Awake()
    {
        DontDestroyOnLoad(this);
    }//End of Awake

    public void setupCmdArgs(string fileName, string sessionID, bool noGraphics)
    {
       
        MetricsRecorder.fileName = fileName;
        MetricsRecorder.sessionID = sessionID;
        MetricsRecorder.graphics = !noGraphics;
        HeaderInfo();

    }//end of setupCmdArgs
    private void HeaderInfo()
    {
        string header = "";
        header += "SenseNodes,MuscleNodes,ControllerNodes,StemNodes,TotalNodes,";
        header += "AvrgSense,AvrgMuscle,AvrgControl,AvrgStem,AvrgNodes,TotalAssemblies,";
        compositionHeader = header;
    }//End of HeaderInfo

    /*Write Metrics writes to the file Metrics Recorder has been given, and if the file doesn't already exist,
     * writes a header, the file is comma seperated values.
     * we only start recording after the first reproduction since it otherwise skews data with the variable time 
     * for reproduction to start occuring.*/
    public void WriteMetrics(string composition, int successfulReproductionsCount, int assemblyID)
    {
        if (successfulReproductionsCount == 1)
        {
            this.startTime = Time.time;
           
        }
        else
        {
            string path = fileName;
            if (!File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path))
                {
                    //Header
                    sw.WriteLine("Time,Reproductions,AverageTime," + compositionHeader + "id,SessionID,Graphics,");
                }
            }
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.Write(Time.time - startTime + ",");
                sw.Write(successfulReproductionsCount + ",");
                sw.Write((Time.time - startTime) / (successfulReproductionsCount - 1) + ",");
                sw.Write(composition);
                sw.Write(assemblyID + ",");
                sw.Write(sessionID + ",");
                sw.Write(graphics + ",");
                sw.WriteLine();
            }
        }
    }//End of WriteMetrics



}
