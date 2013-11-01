using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ConsoleScript : MonoBehaviour {

    public static List<string> lines = new List<string>();
    string inputText = "";

    public static bool active = false;
    public bool consoleKeyCleared = false;


	// Use this for initialization
	void Update (){
        if(!active){
            consoleKeyCleared = false;
            if(WesInput.GetKeyDown("Open Console")){
                active = true;
                WesInput.active = false;
            }
        }
        else if(active){
            if(Input.GetKeyDown(KeyCode.Backspace) && (inputText.Length > 0))
                inputText = inputText.Substring(0, inputText.Length - 2);

	        inputText += Input.inputString;

            if(!consoleKeyCleared){
                inputText = "";
                consoleKeyCleared = true;
            }

            print(">" + inputText + "<");

            // Enter a line.
            if(Input.GetKeyDown(KeyCode.Return)){

                string[] commandArgs = inputText.Split(' ');
                for(int i = 0; i < commandArgs.Length; i++){
                    commandArgs[i] = commandArgs[i].Trim();
                    print(commandArgs[i]);
                }
                string command = commandArgs[0];


                // Console commands --------------------------------------------------------

                if(command == "load"){
                // Load an assembly (by index or name).
                    string loadAssemblyName = "";
                    int loadAssemblyNum = 0;

                    DirectoryInfo dir = new DirectoryInfo("C:/Assembly/saves");
                    FileInfo[] info = dir.GetFiles("*.*");
                    bool foundFile = false;

                    // Load assembly by index...
                    if(int.TryParse(commandArgs[1], out loadAssemblyNum)){
                        NewLine("Attempting to load index " + loadAssemblyNum + "...");
                    
                        for(int i = 0; i < info.Length; i++){
                            FileInfo currentFile = info[i];
                            string currentFileName = currentFile.Name;
                            int currentFileNum = int.Parse(currentFileName.Substring(0, 3));
                            if(currentFileNum == loadAssemblyNum){
                                GameManager.GetAssembly("C:/Assembly/saves/" + currentFileName);
                                NewLine("Done.");
                                foundFile = true;
                                break;
                            }
                        }
                        if(!foundFile) 
                            NewLine("Assembly not found in C:/Assembly/saves/ at index " + loadAssemblyNum + ".");
                    }
                    // Load assembly by name...
                    else if(commandArgs.Length > 0){
                        loadAssemblyName = commandArgs[1];
                        NewLine("Attempting to load '" + loadAssemblyName + "'...");

                        for(int i = 0; i < info.Length; i++){
                            FileInfo currentFile = info[i];
                            string currentFileName = currentFile.Name;
                            if((currentFileName.Length > (loadAssemblyName.Length + 4)) && currentFileName.Substring((currentFileName.Length - loadAssemblyName.Length) - 4, loadAssemblyName.Length) == loadAssemblyName){
                                GameManager.GetAssembly("C:/Assembly/saves/" + currentFileName);
                                NewLine("Done.");
                                foundFile = true;
                                break;
                            }
                        }
                        if(!foundFile)
                            NewLine("Assembly '" + loadAssemblyName + "' not found in C:/Assembly/saves/.");
                    }
                    else
                        NewLine("Enter a save index or Assembly name.");
                }

                else if(command == "clear"){
                // Clear entire simulation
                    GameManager.ClearAll();
                    NewLine("Cleared the world.");
                }

                else if(command == "orbit"){
                // Enable/disable auto-orbiting.
                    if(CameraControl.autoOrbit == Quaternion.identity){
                        CameraControl.OrbitOn();
                        NewLine("Auto-orbit enabled.");
                    }
                    else{
                        CameraControl.OrbitOff();
                        NewLine("Auto-orbit disabled.");
                    }
                }

                else if(command == "save"){
                // Save selected assembly...
                    Assembly selectedAssembly = CameraControl.selectedAssembly;
                    if(selectedAssembly != null){
                        NewLine("Assembly saved to " + selectedAssembly.Save());
                    }
                    else
                        NewLine("No assembly selected!");
                }

                else if(command == "quit"){
                // Quit the game...
                    Application.Quit();
                    NewLine("Quitting...");
                }

                else if(command == "delete_system32"){
                // Destroy Windows...
                    // Please don't put code here...
                }

                else if(command == "other"){
                // Other command...
                    // Other command code...
                }

                // More commands go here!

                else if(inputText != "")
                // If command is not recognized, say so and do nothing.
                    NewLine("Unknown command '" + command + "'");
                
                // (If no command was entered, display no response.)

                inputText = "";
                active = false;
                WesInput.active = true;
            }
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

        bool blinker = ((Time.time % 0.5) <= 0.25);
        if(active && blinker)
            consoleText += "|";
	
        Rect consoleRect = new Rect(5, 0, Screen.width, Screen.height);
        GUI.skin.label.alignment = TextAnchor.LowerLeft;
        GUI.Label(consoleRect, consoleText);

	} // End of OnGUI().


    public static void NewLine(string lineText){
        lines.Add(lineText);
    } // End of NewLine().
}
