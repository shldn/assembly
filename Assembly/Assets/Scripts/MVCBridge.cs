using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using UnityEngine;

public class MVCBridge {

    // Viewer variables
    static TcpClient viewerClient = null;
    static DateTime lastMessageTime = DateTime.Now;
    static Thread viewerReaderThread = null;
    static volatile bool viewerConnectionLost = false;
    static volatile bool viewerReaderThreadStop = false;
    public static volatile bool viewerReadyToSend = true;
    public static volatile bool viewerDataReadyToApply = false;
    public static ViewerData viewerData = null;
    public static bool ViewerConnectionLost { get { return viewerConnectionLost; } }


    // Controller variables
    static TcpListener controllerServer = null;
    public static volatile bool controllerReadyToSend = true;

    // Shared variables
    static int port = 12000;
    static NetworkStream stream = null;



    // Controller Functions
    //--------------------------------------------------------------------
#region ControllerFunctions
    public static void InitializeController() {

        IPAddress localAddr = IPAddress.Parse("127.0.0.1");

        Console.WriteLine("Connecting to port: " + port);
        controllerServer = new TcpListener(localAddr, port);

        // Start listening for client requests.
        controllerServer.Start();
        ConnectToNextViewer();
    }

    private static void ConnectToNextViewer() {
        stream = null;
        controllerServer.BeginAcceptTcpClient(new AsyncCallback(AcceptConnectionFromViewer), controllerServer);
    }

    private static void AcceptConnectionFromViewer(IAsyncResult ar) {

        TcpListener listener = (TcpListener)ar.AsyncState;
        TcpClient client = controllerServer.EndAcceptTcpClient(ar);
        Debug.LogError("Client connected!");
        stream = client.GetStream();

        // should handle this via callback... too specific to this case?
        NodeController.Inst.SendWorldInitDataToViewer();
    }

    public static void SendDataToViewer() {
        if (stream == null) {
            ViewerData.Inst.Clear();
            return;
        }

        // check for messages first -- optimize me!
        if (stream.DataAvailable) {
            List<object> messages = new List<object>();
            IFormatter formatter = new BinaryFormatter();
            try {
                messages = (List<object>)formatter.Deserialize(stream);
            }
            catch (EndOfStreamException e) {
                Debug.LogError("Error reading data from viewer " + e.ToString());
            }
            finally {
                ControllerData.Inst.HandleMessages(messages);
            }
        }

        StartSendDataToViewerThread(ViewerData.Inst);
        ViewerData.Inst.Swap();
        ViewerData.Inst.Clear();
    }

    private static void SendDataToViewerImpl(ViewerData data)
    {
        controllerReadyToSend = false;
        IFormatter formatter = new BinaryFormatter();
        try {
            formatter.Serialize(stream, data);
        }
        catch(Exception e) {
            Debug.LogError("SendDataToViewer failed: " + e.ToString() + "\nAssume the connection was lost");
            ConnectToNextViewer();
        }
        controllerReadyToSend = true;
    }

    private static Thread StartSendDataToViewerThread(ViewerData data)
    {
        Thread t = new Thread(() => SendDataToViewerImpl(data));
        t.Start();
        return t;
    }
    #endregion



    // Viewer Functions
    //--------------------------------------------------------------------
    #region ViewerFunctions
    public static void InitializeViewer() {

        viewerClient = new TcpClient();
        AttemptConnectionToController();
    }

    private static void AttemptConnectionToController() {
        viewerClient.BeginConnect(IPAddress.Parse("127.0.0.1"), port,
        new AsyncCallback(ViewerConnectCallback), viewerClient);
    }
    private static void ViewerConnectCallback(IAsyncResult ar) {
        if (!viewerClient.Connected || viewerClient.GetStream() == null) {
            // try again
            AttemptConnectionToController();
            return;
        }
        viewerClient.EndConnect(ar);
        stream = viewerClient.GetStream();
        viewerConnectionLost = false;
        lastMessageTime = DateTime.Now;
        KillViewerReaderThread();
        viewerDataReadyToApply = false;
        viewerReaderThread = new Thread(new ThreadStart(ViewerReceiveLoop));
        viewerReaderThread.Start();

    }

    public static ViewerData GetDataFromController() {
        return GetDataFromControllerImpl();
    }

    static ViewerData GetDataFromControllerImpl()
    {
        if (stream == null) 
            return null;
        if ((DateTime.Now - lastMessageTime).TotalSeconds > 2)
        {
            Debug.LogError("Haven\'t heard from server for over 2 seconds, looks like server is down, disconnecting.");
            viewerConnectionLost = true;
            return null;
        }
        if (!stream.DataAvailable)
            return null;
        lastMessageTime = DateTime.Now;
        IFormatter formatter = new BinaryFormatter();
        ViewerData data = null;
        try {
            data = (ViewerData)formatter.Deserialize(stream);
        }
        catch(EndOfStreamException e) {
            viewerConnectionLost = true;
            Debug.LogError("Error reading data from controller, assume connection lost " + e.ToString());
        }
        return data;
    }

    static void ViewerReceiveLoop()
    {
        while (!viewerReaderThreadStop)
        {
            if(!viewerDataReadyToApply)
            {
                viewerData = GetDataFromControllerImpl();
                if (viewerData != null)
                    viewerDataReadyToApply = true;
            }
            Thread.Sleep(10);
        }
    }

    static void KillViewerReaderThread()
    {
        viewerReaderThreadStop = true;
        if (viewerReaderThread != null)
            viewerReaderThread.Join();
        viewerReaderThread = null;
        viewerReaderThreadStop = false;
    }

    public static void SendDataToController() {

        if (!ControllerData.Inst.HasData || stream == null) {
            ControllerData.Inst.Clear();
            return;
        }

        StartSendDataToControllerThread(ControllerData.Inst);
        ControllerData.Inst.Swap();
        ControllerData.Inst.Clear();
    }


    private static void SendDataToControllerImpl(ControllerData data) {
        viewerReadyToSend = false;
        IFormatter formatter = new BinaryFormatter();
        try {
            formatter.Serialize(stream, data.Messages);
        }
        catch (Exception e) {
            Debug.LogError("SendDataToViewer failed: " + e.ToString() + "\nAssume the connection was lost");
        }
        viewerReadyToSend = true;
    }

    private static Thread StartSendDataToControllerThread(ControllerData data) {
        Thread t = new Thread(() => SendDataToControllerImpl(data));
        t.Start();
        return t;
    }








    public static void HandleViewerConnectionLost() {
        viewerClient.Close();
        stream = null;
        ViewerController.Inst.Clear();
        InitializeViewer();
        viewerConnectionLost = false;
    }

    public static void CloseViewerConnection() {
        if (viewerClient != null)
            viewerClient.Close();
        KillViewerReaderThread();        
    }

#endregion

}
