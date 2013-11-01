using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ConsoleScript : MonoBehaviour {

    List<string> lines = new List<string>();
    string inputText = "";

    bool active = false;


	// Use this for initialization
	void Update () {
	    inputText += Input.inputString;

        // Enter a line.
        if(Input.GetKeyDown(KeyCode.Return)){

            string[] commandArgs = inputText.Split(' ');
            for(int i = 0; i < commandArgs.Length; i++){
                commandArgs[i] = commandArgs[i].Trim();
                print(commandArgs[i]);
            }
            string command = commandArgs[0];

            // Load an assembly (by index or name).
            if(command == "load"){
                string loadAssemblyName = "";
                int loadAssemblyNum = 0;

                DirectoryInfo dir = new DirectoryInfo("C:/Assembly/saves");
                FileInfo[] info = dir.GetFiles("*.*");
                bool foundFile = false;

                // Load assembly by index...
                if(int.TryParse(commandArgs[1], out loadAssemblyNum)){
                    lines.Add("Attempting to load index " + loadAssemblyNum + "...");
                    
                    for(int i = 0; i < info.Length; i++){
                        FileInfo currentFile = info[i];
                        string currentFileName = currentFile.Name;
                        int currentFileNum = int.Parse(currentFileName.Substring(0, 3));
                        if(currentFileNum == loadAssemblyNum){
                            GameManager.GetAssembly("C:/Assembly/saves/" + currentFileName);
                            lines.Add("Done.");
                            foundFile = true;
                            break;
                        }
                    }
                    if(!foundFile) 
                        lines.Add("Assembly not found in C:/Assembly/saves/ at index " + loadAssemblyNum + ".");
                }
                // Load assembly by name...
                else if(commandArgs.Length > 0){
                    loadAssemblyName = commandArgs[1];
                    lines.Add("Attempting to load '" + loadAssemblyName + "'...");

                    for(int i = 0; i < info.Length; i++){
                        FileInfo currentFile = info[i];
                        string currentFileName = currentFile.Name;
                        if((currentFileName.Length > (loadAssemblyName.Length + 4)) && currentFileName.Substring((currentFileName.Length - loadAssemblyName.Length) - 4, loadAssemblyName.Length) == loadAssemblyName){
                            GameManager.GetAssembly("C:/Assembly/saves/" + currentFileName);
                            lines.Add("Done.");
                            foundFile = true;
                            break;
                        }
                    }
                    if(!foundFile)
                        lines.Add("Assembly '" + loadAssemblyName + "' not found in C:/Assembly/saves/.");
                }
                else
                    lines.Add("Enter a save index or Assembly name.");
            }
            // Save selected assembly...
            else if(command == "save"){
                // Save code...
            }
            // Quite the game...
            else if(command == "quit"){
                // Quit code...
            }
            // Destroy Windows...
            else if(command == "delete_system32"){
                // Please don't put code here...
            }
            // Other command...
            else if(command == "other"){
                // Other command code...
            }
            // More commands go here!

            // If command is not recognized, say so and do nothing.
            else
                lines.Add("Unknown command '" + inputText + "'");
            
            inputText = "";
        }
	}
	
	// Update is called once per frame
	void OnGUI () {

        string consoleText = "";

        for(int i = 0; i < lines.Count; i++){
            string currentLine = lines[i];
            consoleText += currentLine + "\n";
        }

        consoleText += "\n" + inputText;
	
        Rect consoleRect = new Rect(5, 0, Screen.width, Screen.height);
        GUI.skin.label.alignment = TextAnchor.LowerLeft;
        GUI.Label(consoleRect, consoleText);

	}
}
