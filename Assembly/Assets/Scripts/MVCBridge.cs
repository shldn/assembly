using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class MVCBridge {
    static Stream stream = null;
    public static void SendDataToViewer() {
        IFormatter formatter = new BinaryFormatter();
        if(stream == null)
            stream = new MemoryStream();
        stream.Seek(0, SeekOrigin.Begin);
        formatter.Serialize(stream, ViewerData.Inst);
    }

    public static ViewerData GetDataFromController() {
        IFormatter formatter = new BinaryFormatter();
        stream.Seek(0, SeekOrigin.Begin);
        ViewerData data = (ViewerData)formatter.Deserialize(stream);
        return data;
    }
}
