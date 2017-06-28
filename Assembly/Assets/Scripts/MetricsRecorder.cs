using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class MetricsRecorder : MonoBehaviour {
    private int startTime;
    private static string fileName;
    private static string sessionID;
    private static bool graphics;
    private static string compositionHeader;
    private static string propertiesHeader;

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
        NodeCompHeaderInfo();
        NodePropHeaderInfo();

    }//end of setupCmdArgs
    private void NodeCompHeaderInfo()
    {
        string header = "";
        header += "SenseNodes,MuscleNodes,ControllerNodes,StemNodes,TotalNodes,";
        header += "AvrgSense,AvrgMuscle,AvrgControl,AvrgStem,AvrgNodes,TotalAssemblies,";
        compositionHeader = header;
    }//End of HeaderInfo
    private void NodePropHeaderInfo()
    {
        string header = "";
        header += "FieldOfView,SenseRange,MuscleStrength,OscillateFreq,";
        header += "TorqueStrength,FlailMaxAngle,";
        propertiesHeader = header;

    }
    /*Write Metrics writes to the file Metrics Recorder has been given, and if the file doesn't already exist,
     * writes a header, the file is comma seperated values.
     * we only start recording after the first reproduction since it otherwise skews data with the variable time 
     * for reproduction to start occuring.*/
    public void WriteMetrics(string composition,float[] nodeProperties, int successfulReproductionsCount, int assemblyID,int childrenCount,string constructionDirections)
    {
        if (successfulReproductionsCount == 1)
        {
            this.startTime = Time.frameCount;
           
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
                    sw.WriteLine("Time,Reproductions,AverageTime," + compositionHeader + propertiesHeader+"Children,id,SessionID,Graphics,FPS,FPSTarget,ConstructionBlueprint,");
                }
            }
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.Write(Time.frameCount - startTime + ",");
                sw.Write(successfulReproductionsCount + ",");
                sw.Write((Time.frameCount - startTime) / (successfulReproductionsCount - 1) + ",");
                sw.Write(composition);
                for (int i = 0; i < Node.Num_Node_Properties; i++)
                {
                    sw.Write(nodeProperties[i] + ",");
                }
                sw.Write(childrenCount + ",");
                sw.Write(assemblyID + ",");
                sw.Write(sessionID + ",");
                sw.Write(graphics + ",");
                sw.Write(1.0f / Time.deltaTime + ",");
                sw.Write(Application.targetFrameRate + ",");
                sw.Write(constructionDirections+",");
                sw.WriteLine();
            }
        }
    }//End of WriteMetrics



}
