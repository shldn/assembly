using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

public class CopyConfig : MonoBehaviour {
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuildProject) {
        if(target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64) {
            int fileStartIdx = pathToBuildProject.LastIndexOf('/');
            string path = pathToBuildProject.Substring(0, fileStartIdx + 1);
            Debug.Log("Build Post Process - copying config file to " + path);
            FileUtil.ReplaceFile("config.txt", path + "config.txt");
        }
    }
}
