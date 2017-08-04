using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Build : MonoBehaviour {

    static string sceneFolder = "Assets/Scenes/";
    [MenuItem("Assembly/Build Server")]
    public static void BuildServer() {
        string[] scenes = new string[] {sceneFolder + "Logo.unity",
                                        sceneFolder + "Soup_Assemblies.unity",
                                        sceneFolder + "Soup_Amalgams.unity",
                                        sceneFolder + "JellyfishGrotto.unity",
                                        sceneFolder + "RestartSoup.unity"
        };
        string error = BuildPipeline.BuildPlayer(scenes, "./bin/assembly_server.exe", BuildTarget.StandaloneWindows, BuildOptions.AutoRunPlayer);
    }

    [MenuItem("Assembly/Build Client")]
    public static void BuildClient() {
        string[] scenes = new string[] {sceneFolder + "CaptureClient.unity"};
        string error = BuildPipeline.BuildPlayer(scenes, "./bin/assembly_client.exe", BuildTarget.StandaloneWindows, BuildOptions.AutoRunPlayer);
    }

    [MenuItem("Assembly/Build Cognogenesis")]
    public static void BuildCogno() {
        string[] scenes = new string[] { sceneFolder + "Cognogenesis.unity" };
        string error = BuildPipeline.BuildPlayer(scenes, "./bin/assembly_cogno.exe", BuildTarget.StandaloneWindows, BuildOptions.AutoRunPlayer);
    }

    [MenuItem("Assembly/Build Light Server")]
    public static void BuildLightServer() {
        string[] scenes = new string[] { sceneFolder + "LightSoup.unity" };
        string error = BuildPipeline.BuildPlayer(scenes, "./bin/assembly_light.exe", BuildTarget.StandaloneWindows, BuildOptions.AutoRunPlayer);
    }

}
