using UnityEngine;
using System.Collections;

public class DownloadHelper : MonoBehaviour {
    
    public delegate void DownloadCallbackDelegate(WWW downloadObj);
    WWW www = null;
    public void StartDownload(string url, DownloadCallbackDelegate callback)
    {
        StartCoroutine(DownloadProgress(url, callback));
    }

    IEnumerator DownloadProgress(string url, DownloadCallbackDelegate callback)
    {
        www = new WWW(url);
        if (!string.IsNullOrEmpty(www.error))
            Debug.LogError(www.error);
        if( !www.isDone )
            yield return www;

        callback(www);
        www = null;
        Destroy(this);
    }
}
