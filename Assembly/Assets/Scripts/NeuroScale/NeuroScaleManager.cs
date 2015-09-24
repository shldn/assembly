using UnityEngine;
using System.Collections;
using System.Net;
using Boomlagoon.JSON;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Collections.Generic;
using System;

//----------------------------------------------------------//
// NeuroScaleManager
//
// This class handles connection and communication with the NeuroScale API via MQTT messages
// At the moment, you must create an instance via curl and copy & paste in the clientID and subscribe_topic below
//
// Process in curl to get clientID & topic url
//
// // create -- this returns the clientID and subscribe_topic
// curl -i -H 'Content-Type: application/json' -H 'Authorization: Bearer xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx' -X POST -d '{"persist":{"dataset_in":false,"dataset_out":false,"model":false},"pipeline":"pl_nZcKxcLPkmpcPCskUcbRAZ"}' http://api2.neuroscale.io/v1/instances
//
// // poll state -- replace end of url with clientID
// curl -i -H 'Authorization: Bearer xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx' http://api2.neuroscale.io/v1/instances/in_xxxxxxxxxxxxxxxxxxxxxx
//
// // delete -- replace end of url with clientID
// curl -i -H 'Authorization: Bearer xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx' -X DELETE http://api2.neuroscale.io/v1/instances/in_xxxxxxxxxxxxxxxxxxxxxx
//
// Arthur C. Clarke Center for Human Imagination, UCSD
//----------------------------------------------------------//
public class MessageEventArgs : System.EventArgs
{
    public MessageEventArgs(string msg_) { msg = msg_; }
    public string msg;
}

public enum NeuroPipeline{
    ECHO,
    ATTENTION,
    HARDCODED,
    QUERY,
}

public class NeuroScaleManager : MonoBehaviour {

    private static NeuroScaleManager mInst = null;
    public static NeuroScaleManager Inst{
        get {
            if (mInst == null)
                mInst = (new GameObject("NeuroScaleManager")).AddComponent(typeof(NeuroScaleManager)) as NeuroScaleManager;
            return mInst;
        }
    }

    // PASTE YOUR ACCESS TOKEN HERE!
    private string access_token = "23b0a059-7172-47fb-95ca-8fde8df4abb2";  // Unique token per client, only change once - Provided by NeuroScale folks

    private string broker_address = "streaming.neuroscale.io";
    private int broker_port = 443; // 1883
    private string clientID = "in_xxxxxxxxxxxxxxxxxxxxxx";
    private string subscribe_topic = "/nvo6hBF9xVsq3McM8rEWjE/out/default";
    private string publish_topic = "/xxxxxxxxxxxxxxxxxxxxxx/in";
    private string instance_clientID = "in_xxxxxxxxxxxxxxxxxxxxxx";

    // When querying for running instances, it will search for this tag.
    private string instanceTag = "Assembly";

    // Pipeline
    NeuroPipeline pipeline = NeuroPipeline.QUERY;
    bool CreatePipeline { get { return pipeline != NeuroPipeline.QUERY && pipeline != NeuroPipeline.HARDCODED; } }

    // Communication helpers
    private MqttClient mqttClient = null;
    private RESTHelper restHelper = null;

    // state bools
    private bool instanceRunning = false;
    private bool connected = false;

    // Timing measurements
    DateTime initTime = DateTime.Now;
    
    // Events
    public delegate void MessageEventHandler(object sender, MessageEventArgs e);
    public event MessageEventHandler Messages;

    // Accessors
    public string PublishTopic { get { return publish_topic; } }
    public bool Connected { get { return connected; } }

    void Awake()
    {
#if UNITY_STANDALONE 

        // create client instance
        string brokerIP = Dns.GetHostAddresses(broker_address)[0].ToString();
        mqttClient = new MqttClient(brokerIP, broker_port, false, null);

        // register mqtt events
        mqttClient.MqttMsgPublishReceived += OnMQTTMessage;
        mqttClient.ConnectionClosed += OnConnectionClosed;
        mqttClient.MqttMsgSubscribed += OnSubscribed;
        mqttClient.MqttMsgUnsubscribed += OnUnsubscribed;
        mqttClient.MqttMsgPublished += OnPublished;

        // random client id -- duplicates can cause issues -- overwritten if creating a new pipeline
        clientID = "in_" + System.IO.Path.GetRandomFileName().Replace(".", "") + System.IO.Path.GetRandomFileName().Replace(".", "");

        // initialize instance
        restHelper = new RESTHelper("https://api.neuroscale.io/v1/instances", new Dictionary<string, string>() { { "Authorization", "Bearer " + access_token } });
        InitializeInstance();
        InvokeRepeating("ConnectAndSubscribe",3f, 3f);


#endif
    }

    void InitializeInstance()
    {
        if (pipeline == NeuroPipeline.ATTENTION)
            InitializeAttentionInstance();
        else if (pipeline == NeuroPipeline.ECHO)
            InitializeEchoInstance();
        else if (pipeline == NeuroPipeline.HARDCODED)
            InitializeHardCodedInstance();
        else if (pipeline == NeuroPipeline.QUERY)
            InitializeQueryInstance();

        initTime = DateTime.Now;
    }

    // Send request to setup an echo instance, parse the response.
    void InitializeEchoInstance()
    {
        string response = restHelper.sendRequest("", "POST", "{\"persist\":{\"dataset_in\":false,\"dataset_out\":false,\"model\":false},\"pipeline\":\"pl_nZcKxcLPkmpcPCskUcbRAZ\"}");
        SetDataFromInitResponse(response);

    }

    // Send request to setup an attention instance, parse the response.
    void InitializeAttentionInstance()
    {
        string response = restHelper.sendRequest("", "POST", "{\"persist\":{\"dataset_in\":false,\"dataset_out\":false,\"model\":false},\"pipeline\":\"pl_attention\",\"properties\":{\"tag\":\"Attention Instance\",\"playername\":\"myplayer\"}}");
        SetDataFromInitResponse(response);
    }

    void InitializeHardCodedInstance()
    {
        ConnectAndSubscribe();
    }

    void InitializeQueryInstance()
    {
        string response = restHelper.sendRequest("", "GET", "where={\"tag\":\"" + instanceTag + "\"}");
        Debug.LogError("Query response: " + response);
        JSONObject jsonObj = JSONObject.Parse(response);
        JSONArray array = jsonObj.GetArray("data");
        for (int i = 0; i < array.Length; ++i)
            if (array[i].ToString().Contains(instanceTag))
                SetDataFromInitResponse(array[i].Obj);
        ConnectAndSubscribe();
    }

    
    void SetDataFromInitResponse(string responseJSON)
    {
        JSONObject jsonObj = JSONObject.Parse(responseJSON);
        SetDataFromInitResponse(jsonObj);
    }

    void SetDataFromInitResponse(JSONObject jsonObj)
    {
        if (jsonObj != null && jsonObj.ContainsKey("id"))
        {
            instance_clientID = jsonObj.GetString("id");
            Debug.Log("Client ID: " + instance_clientID);
        }
        if (CreatePipeline)
            clientID = instance_clientID;

        if (jsonObj != null && jsonObj.ContainsKey("endpoints"))
        {
            JSONObject endpointsObj = jsonObj.GetObject("endpoints");
            if (endpointsObj != null && endpointsObj.ContainsKey("data"))
            {
                JSONArray dataArr = endpointsObj.GetArray("data");
                if (dataArr.Length > 0 && dataArr[0].Obj.ContainsKey("url"))
                {
                    string url = dataArr[0].Obj.GetString("url");
                    int splitIdx = url.IndexOf('/', 8);
                    subscribe_topic = url.Substring(splitIdx) + "/default";
                    Debug.Log("sub topic: " + subscribe_topic);
                }
                if (dataArr.Length > 1 && dataArr[1].Obj.ContainsKey("url"))
                {
                    string url = dataArr[1].Obj.GetString("url");
                    int splitIdx = url.IndexOf('/', 8);
                    publish_topic = url.Substring(splitIdx);
                    Debug.Log("publish topic: " + publish_topic);
                }
            }
        }
    }

    // Check if the instance is in a running state. Cannot receive messages until it is "running"
    bool IsInstanceRunning()
    {
        if (instanceRunning)
            return true;

        Debug.Log("Checking if instance ready...");
        string response = restHelper.sendRequest(instance_clientID, "GET");
        JSONObject jsonObj = JSONObject.Parse(response);
        if (jsonObj != null && jsonObj.ContainsKey("state"))
            instanceRunning = jsonObj.GetString("state") == "running";
        return instanceRunning;
    }

    void ConnectAndSubscribe()
    {
        if (pipeline != NeuroPipeline.HARDCODED && !IsInstanceRunning())
            return;

        Debug.Log("Connecting and subscribing - " + (DateTime.Now - initTime).TotalSeconds + " secs to initialize.");
        mqttClient.Connect(clientID, "", "", true, 120);
        mqttClient.Subscribe(new string[] { subscribe_topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        connected = true;
        CancelInvoke();
    }

    void CloseEchoInstance()
    {
        Debug.Log("Closing Echo Instance");
        restHelper.sendRequest(instance_clientID, "DELETE");
    }

    void OnDestroy()
    {
        // Destroy pipeline, if we created it.
        if(CreatePipeline)
            CloseEchoInstance();
    }


    void OnMQTTMessage(object sender, MqttMsgPublishEventArgs e)
    {
        string msg = System.Text.Encoding.Default.GetString(e.Message);

        // Propagate event
        if(Messages != null)
            Messages(this, new MessageEventArgs(msg));
    }

    void OnConnectionClosed(object sender, System.EventArgs e)
    {
        Debug.Log("OnConnectionClosed: " + e.ToString());
        Debug.Log("Reconnecting...");
        connected = false;
        ConnectAndSubscribe();
    }

    void OnPublished(object sender, MqttMsgPublishedEventArgs e)
    {
        Debug.Log("OnPublished: " + e.ToString());
    }

    void OnSubscribed(object sender, MqttMsgSubscribedEventArgs e)
    {
        Debug.Log("OnSubscribed: " + e.MessageId + " " + e.ToString());
    }

    void OnUnsubscribed(object sender, MqttMsgUnsubscribedEventArgs e)
    {
        Debug.Log("OnUnsubscribed: " + e.MessageId + " " + e.ToString());
    }

    void OnApplicationQuit()
    {
        Debug.LogError("Application closed.");
    }

}
