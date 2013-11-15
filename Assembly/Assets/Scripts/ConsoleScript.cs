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
        string command = "";
        string[] commandArgs = new string[0];


        if(!active){
            consoleKeyCleared = false;
            if(WesInput.GetKeyDown("Open Console")){
                active = true;
                WesInput.active = false;
            }

            // Keyboard shortcuts
            if(WesInput.GetKeyDown("Disband Assembly"))
                command = "disband";

            // Keyboard shortcuts
            if(WesInput.GetKeyDown("Add Node"))
                command = "addnode";

            // Keyboard shortcuts
            if(WesInput.GetKeyDown("Remove Node"))
                command = "remnode";

        }
        else if(active){
            if(Input.GetKeyDown(KeyCode.Backspace) && (inputText.Length > 0))
                inputText = inputText.Substring(0, inputText.Length - 1);

            foreach(char aChar in Input.inputString){
	            if(aChar != '\b'){
                    inputText += aChar;
                }
            }

            if(!consoleKeyCleared){
                inputText = "";
                consoleKeyCleared = true;
            }

            // Enter a line.
            if(Input.GetKeyDown(KeyCode.Return)){
                if(inputText != ""){
                    commandArgs = inputText.Split(' ');
                    for(int i = 0; i < commandArgs.Length; i++){
                        commandArgs[i] = commandArgs[i].Trim();
                    }
                    command = commandArgs[0];
                }
                else{
                    // (If no command was entered, display no response.)
                    inputText = "";
                    active = false;
                    WesInput.active = true;
                }
            }
        }
        
        if(command != ""){
            // Console commands --------------------------------------------------------

            Node selectedNode = CameraControl.selectedNode;
            Assembly selectedAssembly = CameraControl.selectedAssembly;

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
                            new Assembly("C:/Assembly/saves/" + currentFileName);
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
                            new Assembly("C:/Assembly/saves/" + currentFileName);
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
                Clear();
            }

            else if (command == "loaddir") {
                if (commandArgs.Length > 0)
                    GameManager.LoadDirectory(commandArgs[1]);
                else
                    NewLine("No directory path found");
                Clear();
            }

            else if(command == "clear"){
            // Clear entire simulation
                GameManager.ClearAll();
                NewLine("Cleared the world.");
                Clear();
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
                Clear();
            }

            else if(command == "save"){
            // Save selected assembly...
                if(selectedAssembly != null){
                    NewLine("Assembly saved to " + selectedAssembly.Save());
                }
                else
                    NewLine("No assembly selected!");
                Clear();
            }

            else if (command == "addnode") {
                // Other command...
                GameObject newNode = Object.Instantiate(GameManager.prefabs.node, Camera.main.transform.position + (Camera.main.transform.forward * 3.0f), Quaternion.identity) as GameObject;
                Node nodeScript = newNode.GetComponent<Node>();
                lines.Add("Created a new node.");
                if (selectedNode) {
                    if (selectedNode.bonds.Count < 3)
                        new Bond(CameraControl.selectedNode, nodeScript);
                }

                CameraControl.selectedNode = nodeScript;
                Clear();

            }
            else if (command == "mutate") {
                // Mutate the selected assembly...
                if (selectedAssembly != null) {
                    selectedAssembly.Mutate();
                    NewLine("Assembly " + selectedAssembly.name + " mutated!");
                }
                else
                    NewLine("No assembly selected!");
                Clear();
            }

            else if (command == "quit") {
                // Quit the game...
                Application.Quit();
                NewLine("Quitting...");
                Clear();
            }

            else if (command == "remnode") {
                if (selectedNode) {
                    selectedNode.Destroy();
                    lines.Add("Removed a node.");
                }
                else
                    lines.Add("Select a node first!");

                Clear();
            }

            else if (command == "disband") {
                if (selectedAssembly != null) {
                    lines.Add("Disbanded " + selectedAssembly.name + ".");
                    selectedAssembly.Disband();
                }
                else
                    lines.Add("Select an assembly first!");

                Clear();
            }

            else if (command == "rename") {
                if (selectedAssembly == null)
                    lines.Add("Select an assembly first!");
                else if ((commandArgs.Length < 2) || (commandArgs[1] == ""))
                    lines.Add("Please enter a name for the assembly.");
                else if (selectedAssembly != null) {
                    lines.Add("Renamed " + selectedAssembly.name + " to " + commandArgs[1] + ".");
                    selectedAssembly.name = commandArgs[1];
                }
                Clear();
            }

            else if (command == "controls") {
                foreach (KeyValuePair<string, KeyCode> item in WesInput.keys) {
                    lines.Add(item.Key + " [" + item.Value + "]");
                }
                Clear();
            }

            else if (command == "other") {
                // Other command...
                // Other command code...
                Clear();
            }


            // More commands go here!

            else if (inputText != "") {
                // If command is not recognized, say so and do nothing.
                NewLine("Unknown command '" + command + "'");
                Clear();
            }
        }
	}


    void Clear(){
        inputText = "";
        active = false;
        WesInput.active = true;
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
