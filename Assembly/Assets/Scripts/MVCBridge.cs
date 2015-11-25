using System;
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

    // Controller variables
    static TcpListener controllerServer = null;

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

        IFormatter formatter = new BinaryFormatter();
        try {
            formatter.Serialize(stream, ViewerData.Inst);
        }
        catch(Exception e) {
            Debug.LogError("SendDataToViewer failed: " + e.ToString() + "\nAssume the connection was lost");
            ConnectToNextViewer();
        }
        ViewerData.Inst.Clear();
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
        if (!viewerClient.Connected) {
            // try again
            AttemptConnectionToController();
            return;
        }
        viewerClient.EndConnect(ar);
        stream = viewerClient.GetStream();
    }

    public static ViewerData GetDataFromController() {
        if (stream == null || !stream.DataAvailable)
            return null;
        IFormatter formatter = new BinaryFormatter();
        ViewerData data = null;
        try {
            data = (ViewerData)formatter.Deserialize(stream);
        }
        catch(EndOfStreamException e) {
            Debug.LogError("Error reading data from controller, assume connection lost " + e.ToString());
            viewerClient.Close();
            stream = null;
            InitializeViewer();
        }
        return data;
    }

    public static void CloseViewerConnection() {
        if (viewerClient != null)
            viewerClient.Close();
    }

#endregion

}
