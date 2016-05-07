﻿using UnityEngine;
using System;
using System.IO;
using Boomlagoon.JSON;
using System.Collections.Generic;

public class IpPortPair
{
    public IpPortPair(string ip_, int port_) { ip = ip_;  port = port_; }
    public string ip;
    public int port;
}

public static class Config {

    // Graphics Config


    // Viewer Config
    public static List<IpPortPair> controllerAddress = new List<IpPortPair>();


    // Controller Config


    // EEG Config
    public static string LSLStream = "ConcentrationStream";


    static Config () {
        Read("config.txt");
	}

    public static void Touch() { }
	





    // Read functions
    //-----------------------------------------------------------------------

	static void Read (string filePath) {
        try {
            using (StreamReader sr = new StreamReader(filePath)) {
                string fileStr = sr.ReadToEnd();
                JSONArray arr = JSONArray.Parse(fileStr);
                if (arr == null) {
                    Debug.LogError("Problem parsing " + filePath + " probably not valid JSON");
                    return;
                }
                for (int i = 0; i < arr.Length; ++i) {
                    switch (arr[i].Obj.GetString("type").Trim()) {
                        case "description":
                            break;
                        case "graphics":
                            ReadGraphics(arr[i].Obj);
                            break;
                        case "viewer":
                            ReadViewer(arr[i].Obj);
                            break;
                        case "controller":
                            ReadController(arr[i].Obj);
                            break;
                        case "eeg":
                            ReadEEG(arr[i].Obj);
                            break;
                        default:
                            Debug.LogError("Config type " + arr[i].Obj.GetString("type").Trim() + " not handled yet.");
                            break;
                    }
                }
            }
        }
        catch (Exception e) {
            string errorMsg = "The file could not be read: " + filePath;
            Debug.LogError(errorMsg);
        }
    }

    static void ReadGraphics(JSONObject obj) {

    }

    static void ReadViewer(JSONObject obj) {
        foreach (KeyValuePair<string, JSONValue> v in obj) {
            if (v.Key.Trim() == "controllers")
                AddControllerAddresses(v.Value);
        }
    }

    static void ReadController(JSONObject obj) {

    }

    static void ReadEEG(JSONObject obj) {
        foreach (KeyValuePair<string, JSONValue> v in obj) {
            if (v.Key.Trim() == "LSLStream")
                LSLStream = v.Value.Str.Trim();
        }
    }

    static void AddControllerAddresses(JSONValue v) {
        foreach (JSONValue val in v.Array) {
            controllerAddress.Add(new IpPortPair(val.Obj.GetString("ip"), (int)val.Obj.GetNumber("port")));
        }
    }
}