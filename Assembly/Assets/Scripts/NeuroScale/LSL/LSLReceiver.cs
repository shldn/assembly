using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;
using LSL;
using System.Xml;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Net;

class LSLReceiver : MonoBehaviour
{
    liblsl.StreamInlet inlet = null;
    int channelCount = 8;
    int sampleRate = 500;
    string source_id = "";
    string uid = "";

    // Sample storage
    List<double> timestamps = new List<double>();
    List<List<float>> samples = new List<List<float>>();

    // Send to NeuroScale variables
    private MqttClient mqttClient = null;
    private string clientID = "in_6dEXSX64hFEWzpGja8ck9A";
    private string broker_address = "streaming2.neuroscale.io";
    private string publishTopic = "/WFccmH6eyvsrTFYC52EHSP/in";
    private float delayBetweenMessages = 0.1f; // seconds
    private DateTime lastSendTime = DateTime.Now;


    void Awake()
    {
        // Init LSL Reciever
        // wait until an EEG stream shows up
        liblsl.StreamInfo[] results = liblsl.resolve_stream("type", "EEG");

        // open an inlet and print some interesting info about the stream (meta-data, etc.)
        inlet = new liblsl.StreamInlet(results[0]);
        ParseStreamInfo(inlet.info().as_xml());
        
        // Init Storage Data Structures
        InitSampleStorage();

        // Init MQTT sender to Neuroscale
        InitMQTTClient();
    }

    void InitMQTTClient()
    {
        // create client instance
        string brokerIP = Dns.GetHostAddresses(broker_address)[0].ToString();
        mqttClient = new MqttClient(brokerIP, 1883, false, null);
        mqttClient.Connect(clientID, "", "", true, 120);

        // register mqtt events
        mqttClient.MqttMsgPublished += OnPublished;
        mqttClient.ConnectionClosed += OnConnectionClosed;
    }

    void ParseStreamInfo(string infoXML)
    {
        Debug.Log(infoXML);
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(infoXML); 

        // Get elements
        XmlNodeList channelCountNode = xmlDoc.GetElementsByTagName("channel_count");
        XmlNodeList sampleRateNode = xmlDoc.GetElementsByTagName("nominal_srate");
        XmlNodeList sourceNode = xmlDoc.GetElementsByTagName("source_id");
        XmlNodeList uidNode = xmlDoc.GetElementsByTagName("uid");

        channelCount = int.Parse(channelCountNode[0].InnerText);
        sampleRate = int.Parse(sampleRateNode[0].InnerText);
        source_id = sourceNode[0].InnerText;
        uid = uidNode[0].InnerText;

        Debug.Log(channelCount + " " + sampleRate + " " + source_id + " " + uid);
    }

    void InitSampleStorage()
    {
        for (int i = 0; i < channelCount; ++i)
            samples.Add(new List<float>());
    }

    void ClearSampleStorage()
    {
        for (int i = 0; i < samples.Count; ++i)
            samples[i].Clear();
        timestamps.Clear();
    }
    

    void Update()
    {
        // read samples
        float[] sample = new float[channelCount];
        string str = "";

        if( inlet.samples_available() > 0)
            Debug.Log("Samples available: " + inlet.samples_available());
        double timestamp = 0;
        try
        {
            timestamp = inlet.pull_sample(sample);
        }
        catch(LSL.liblsl.LostException e)
        {
            Debug.LogError("The LSL inlet has been lost :/ " + e.ToString());
            return;
        }
        timestamps.Add(timestamp + inlet.time_correction());

        //str += "time: " + (timestamps[timestamps.Count - 1]);
        //foreach (float f in sample)
        //    str += "\t" + f;
        //Debug.Log(str + "\n");
        for (int i = 0; i < channelCount; ++i)
            samples[i].Add(sample[i]);

        if(samples[0].Count >= 12)
        {
            string msg = CreateNeuroscaleMessage(timestamps, samples);
            if ((DateTime.Now - lastSendTime).TotalSeconds > delayBetweenMessages)
                PublishToNeuroScale(msg);
            ClearSampleStorage();
        }
    }

    double GetMeanTimeStamp(List<double> timestamps)
    {
        if (timestamps.Count == 0)
            return 0;
        double sum = 0f;
        foreach (double t in timestamps)
            sum += t;
        return sum / timestamps.Count;
    }

    string GetJSONArrayStr<T>(List<T> data)
    {
        string jsonStr = "[";
        for(int i = 0; i < data.Count; ++i)
        {
            if (i > 0)
                jsonStr += ", ";
            jsonStr += data[i];
        }
        jsonStr += "]";
        return jsonStr;
    }

    string GetSampleJSONStr(List<List<float>> samples, int channels = -1)
    {
        string sampleJSON = "[";
        int max = channels == -1 ? samples.Count : channels;
        for (int i = 0; i < max; ++i)
        {
            if (i > 0)
                sampleJSON += ", ";
            sampleJSON += GetJSONArrayStr(samples[i]);
        }
        sampleJSON += "]";
        return sampleJSON;
    }

    string CreateNeuroscaleMessage(List<double> timestamps, List<List<float>> samples)
    {

        string jsonMsg = "";

        jsonMsg += "{";

        jsonMsg += "    \"streams\": [{";

        jsonMsg += "        \"time_stamps\": " + GetJSONArrayStr(timestamps) + ",";

        jsonMsg += "        \"incremental\": true,";

        jsonMsg += "        \"mean_time_stamp\": " + GetMeanTimeStamp(timestamps) + ","; //567724.05188215699,

        jsonMsg += "        \"samples\": " + GetSampleJSONStr(samples, 4) + ",";

        jsonMsg += "        \"source_id\": \"" + source_id + "\","; 

        jsonMsg += "        \"sampling_rate\": " + sampleRate + ",";

        jsonMsg += "        \"signal\": true,";

        jsonMsg += "        \"modality\": \"EEG\",";

        jsonMsg += "        \"channel\": [\"Tp9\", \"Fp1\", \"Fp2\", \"Tp10\"],";

        jsonMsg += "        \"source_url\": \"lsl://" + source_id + "?uid=" + uid + "\"";

        jsonMsg += "    }";

        jsonMsg += "    ]";

        jsonMsg += "}";

        //Debug.Log(jsonMsg);

        return jsonMsg;

    }

    void PublishToNeuroScale(string msg)
    {
        Debug.Log("Publish to neuroscale: " + msg);
        mqttClient.Publish(publishTopic, System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE,false);
        lastSendTime = DateTime.Now;
    }

    void OnPublished(object sender, MqttMsgPublishedEventArgs e)
    {
        Debug.Log("OnPublished: " + e.ToString());
    }

    void OnConnectionClosed(object sender, System.EventArgs e)
    {
        Debug.Log("OnConnectionClosed: " + e.ToString());
        // reconnect
        mqttClient.Connect(clientID, "", "", true, 120);
    }

    void OnDestroy()
    {
        inlet.close_stream();
        mqttClient.Disconnect();
    }
}