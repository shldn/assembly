using UnityEngine;
using System;
using System.IO;

public class IOHelper{

    public static string defaultSaveDir = "C:/Assembly/saves/";


    public static void CreateDefaultDirectory() {
        Directory.CreateDirectory(defaultSaveDir);
    }


    public static void SaveAllToFolder(string folderPath)
    {
        string ext = ".txt";
        Directory.CreateDirectory(folderPath);
        foreach (Assembly a in Assembly.GetAll()) {
            string filename = folderPath + a.name + ext;
            a.Save(filename);
        }
    }


    public static void LoadDirectory(string dir) {
        if (dir == "")
            return;
        try {
            ConsoleScript.NewLine("Loading directory " + dir);
            string[] filePaths = Directory.GetFiles(dir);
            foreach (string file in filePaths)
                new Assembly(file);
        }
        catch (Exception e) {
            Debug.LogError("LoadDirectory failed: " + e.ToString());
        }
    } // End of LoadDirectory

}
