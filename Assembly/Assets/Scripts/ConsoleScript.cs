using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

// A single command that can be put into the console.
public class ConsoleCommand {

    public string name = "";
    public delegate void CmdFunc(string[] args);
    public CmdFunc func;

    public ConsoleCommand(string _name){
        name = _name;
    } // End of constructor.

    public ConsoleCommand(string _name, CmdFunc _func)
    {
        name = _name;
        func = _func;
    } // End of constructor.

} // End of ConsoleCommand.*/


// Reads console input, registering commands and such.
public class ConsoleScript : MonoBehaviour {

    public static List<string> lines = new List<string>();
    string inputText = "";

    public float logFade = 0f;
    public float logVisibleTime = 0f;

    public static bool active = false;
    public bool consoleKeyCleared = false;

    public Dictionary<string, ConsoleCommand> commands = new Dictionary<string, ConsoleCommand>();

    public static ConsoleScript Inst = null;
    NetworkView myNetworkView = null;


    void Awake(){
        Inst = this;

        myNetworkView = GetComponent<NetworkView>();

        List<ConsoleCommand> cmdList = new List<ConsoleCommand>();
        cmdList.Add(new ConsoleCommand("clear"));
        cmdList.Add(new ConsoleCommand("say"));
        cmdList.Add(new ConsoleCommand("disband"));
        cmdList.Add(new ConsoleCommand("help"));
        cmdList.Add(new ConsoleCommand("load"));
        cmdList.Add(new ConsoleCommand("demo"));
        cmdList.Add(new ConsoleCommand("quit"));
        cmdList.Add(new ConsoleCommand("reload"));
        cmdList.Add(new ConsoleCommand("save"));
        cmdList.Add(new ConsoleCommand("saveselected"));

        RegisterCommands(cmdList);

    } // End of Awake().

    void RegisterCommands(List<ConsoleCommand> cmdList)
    {
        for (int i = 0; i < cmdList.Count; ++i)
            commands.Add(cmdList[i].name, cmdList[i]);

        commands["clear"].func = delegate(string[] args)
        {
            //GameManager.ClearAll();
        };

        commands["disband"].func = delegate(string[] args)
        {

        };

        commands["help"].func = delegate(string[] args)
        {
            WriteToLog("Available Commands:");
            foreach(KeyValuePair<string,ConsoleCommand> cmd in commands)
                WriteToLog("\t" + cmd.Key);
        };

        commands["say"].func = delegate(string[] args)
        {
            // Need to handle all args after 0!
            string messageToSend = "";
            for(int i = 1; i < args.Length; i++)
                messageToSend += args[i] + " ";

            GlobalWriteToLog(Net_Manager.playerName + " \"" + messageToSend + "\"", RPCMode.All);
        };

        /*commands["load"].func = delegate(string[] args)
        {
            if (args.Length <= 1)
                WriteToLog("Please specify a file to load");
            else
            {
                string path = args[1];
                for (int i = 2; i < args.Length; ++i)
                    path += " " + args[i];
                EnvironmentManager.Load(path);
            }
        };*/

        commands["demo"].func = delegate(string[] args)
        {
            /*
            if(MainCameraControl.Inst.camType != CamType.ORBIT_DEMO){
                MainCameraControl.Inst.randomOrbit = Random.rotation;
                MainCameraControl.Inst.camType = CamType.ORBIT_DEMO;
            }
            */
        };

        commands["quit"].func = delegate(string[] args)
        {
			if((Application.platform != RuntimePlatform.Android) || (ClientAdminMenu.Inst && !ClientAdminMenu.Inst.isOpen))
				Application.Quit();
        };

        commands["reload"].func = delegate(string[] args)
        {

        };

        /*commands["save"].func = delegate(string[] args)
        {
            if( args.Length > 1 && (args[1] == "pos" || args[1] == "position") )
                EnvironmentManager.SavePositionsOnly(IOHelper.GetValidFileName("./data/", "env", ".txt"));
            else
                EnvironmentManager.Save(IOHelper.GetValidFileName("./data/", "env", ".txt"));
        };*/

        commands["saveselected"].func = delegate(string[] args)
        {
            /*
            if (MainCameraControl.Inst.selectedAssembly == null)
                WriteToLog("No assembly selected");
            else
            {
                WriteToLog("Writing selected assembly to file");
                MainCameraControl.Inst.selectedAssembly.Save();
            }
            */
        };


    } // End of RegisterCommands().

    void Update()
    {
        string[] commandArgs = new string[0];

        // Log visibility/fade out.
        if(logVisibleTime > 0f)
            logFade = 1f;
        else
            logFade = Mathf.MoveTowards(logFade, 0f, (Time.deltaTime / Time.timeScale));

        logVisibleTime -= (Time.deltaTime / Time.timeScale);

        if(active)
            logVisibleTime = 5f;


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

		if(Input.GetKeyDown(KeyCode.Escape))
			InterperetCommand("quit");
	}


    void InterperetCommand(string input){
        
        lines.Add(input);
        logVisibleTime = 5f;

        // Console stuff here!
        // Combat virbela habit
        if( input.StartsWith("/") )
            input = input.TrimStart(new char[]{'/'});

        // Call command if it is in the command list
        string[] tok = input.Split(' ');
        if (tok.Length > 0 && commands.ContainsKey(tok[0]))
            commands[tok[0]].func(tok);

    } // End of InterperetCommand().

    public void WriteToLog(string text){
        lines.Add(text);
        logVisibleTime = 5f;
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

        GUI.color = new Color(1f, 1f, 1f, logFade);
        Rect consoleRect = new Rect(5, 0, Screen.width, Screen.height - (inputTextRectHeight + 5));
        GUI.skin.label.alignment = TextAnchor.LowerLeft;
        GUI.Label(consoleRect, consoleText);

	} // End of OnGUI().


    public static void NewLine(string lineText){
        lines.Add(lineText);
    } // End of NewLine().


    // Displays a message in the client's chat log and attempts to do the same for all connected players.
    public void GlobalWriteToLog(string message, RPCMode rpcMode){
	    if(Network.peerType != NetworkPeerType.Disconnected)
		    myNetworkView.RPC("ChatMessage", rpcMode, message);
	    else
		    WriteToLog(message);
    } // End of GlobalMessage().


    [RPC] // Displays a networked client message.
    void ChatMessage(string message){
	    WriteToLog(message);
    } // End of ChatMessage().
}



