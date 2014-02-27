using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

// A single command that can be put into the console.
public class ConsoleCommand {

    string name = "";

    public ConsoleCommand(string _name){
        name = _name;
    } // End of constructor.

} // End of ConsoleCommand.*/


// Reads console input, registering commands and such.
public class ConsoleScript : MonoBehaviour {

    public static List<string> lines = new List<string>();
    string inputText = "";

    public static bool active = false;
    public bool consoleKeyCleared = false;

    public List<ConsoleCommand> commands = new List<ConsoleCommand>();


    void Awake(){

        commands.Add(new ConsoleCommand("disband"));
        commands.Add(new ConsoleCommand("orbit"));
        commands.Add(new ConsoleCommand("quit"));
        commands.Add(new ConsoleCommand("reload"));

    } // End of Awake().


	void Update(){
        string[] commandArgs = new string[0];


        if(inputText == "`")
            inputText = "";

        if(Input.GetKeyDown(KeyCode.BackQuote)){
            if(!active){
                active = true;
                inputText = "";
            }
            else
                active = false;
        }

        // Enter a line.
        Input.eatKeyPressOnTextFieldFocus = false;
        if(Input.GetKeyDown(KeyCode.Return)){
            if(inputText != ""){
                InterperetCommand(inputText);
                inputText = "";
            }
            else{
                // (If no command was entered, display no response.)
                inputText = "";
                active = false;
                WesInput.active = true;
            }
        }
	}


    void InterperetCommand(string input){
        
        lines.Add(input);
        // Console stuff here!

    } // End of InterperetCommand().

    public static void WriteToLog(string text){
        lines.Add(text);
    } // End of WriteToConsoleLog().


    void Clear(){
        inputText = "";
        active = false;
        WesInput.active = true;
    } // End of Clear().

	
	// Update is called once per frame
	void OnGUI(){
        // User input field
        int inputTextRectHeight = 50;
        if(active){
            Rect inputTextRect = new Rect(5, Screen.height - (inputTextRectHeight + 5), Screen.width - 10, inputTextRectHeight);
            GUI.SetNextControlName("userInputField");
            inputText = GUI.TextField(inputTextRect, inputText);
            GUI.FocusControl("userInputField");
        }

        string consoleText = "";

        for(int i = 0; i < lines.Count; i++){
            string currentLine = lines[i];
            consoleText += currentLine + "\n";
        }

        Rect consoleRect = new Rect(5, 0, Screen.width, Screen.height - (inputTextRectHeight + 5));
        GUI.skin.label.alignment = TextAnchor.LowerLeft;
        GUI.Label(consoleRect, consoleText);

	} // End of OnGUI().


    public static void NewLine(string lineText){
        lines.Add(lineText);
    } // End of NewLine().
}



