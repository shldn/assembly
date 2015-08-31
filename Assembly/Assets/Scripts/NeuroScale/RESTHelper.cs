using UnityEngine;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Security;
using System.IO;
using System.Security.Cryptography.X509Certificates;

//----------------------------------------------------------//
// RESTHelper
//
//
// Arthur C. Clarke Center for Human Imagination, UCSD
//----------------------------------------------------------//
public class RESTHelper {
	
	public string baseURL = "";
	private Dictionary<string, string> httpHeaders;
	private static WebClient webclient;
	
	public RESTHelper(string url, Dictionary<string, string> headers)
	{
		baseURL = url;
		httpHeaders = headers;
    }
	
	public static bool Validator (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
	{
        Debug.LogError(String.Format("Certificate error: {0}", sslPolicyErrors));
        if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors)
        {
            Debug.LogError("X509ChainStatus Details: ");
#if !UNITY_WEBPLAYER
            foreach (X509ChainStatus cs in chain.ChainStatus)
                Debug.LogError("X509ChainStatus: " + cs.StatusInformation);
#endif
        }
		return true;
	}
	
	public void AddHeader(string k, string v)
	{
		if(!httpHeaders.ContainsKey(k))
			httpHeaders.Add(k, v);
		else
			httpHeaders[k] = v;
	}

    public string sendRequest(string urlSubDir, string method, string data = "", bool waitForResponse = true)
	{
#if UNITY_WEBPLAYER
        return "";
#endif

		string ret = "";
        if (!string.IsNullOrEmpty(urlSubDir) && !urlSubDir.StartsWith("/"))
            urlSubDir = "/" + urlSubDir;
        string url = baseURL + urlSubDir;
		method = method.ToUpper();
		
		if (method == "GET" && data != "")
			url = url + "?" + data;

		//ServicePointManager.ServerCertificateValidationCallback = Validator;
		HttpWebRequest myHttpWebRequest=(HttpWebRequest)WebRequest.Create(url);
		myHttpWebRequest.Method = method;
		foreach(KeyValuePair<string, string> kv in httpHeaders){
			myHttpWebRequest.Headers.Add(kv.Key, kv.Value);
		}
		if (method == "POST" || method == "PUT") 
		{
            Debug.Log("POST - writing data");
			myHttpWebRequest.ContentType = "application/json";
			using (var streamWriter = new StreamWriter(myHttpWebRequest.GetRequestStream()))
			{
				streamWriter.Write(data);
				streamWriter.Flush();
				streamWriter.Close();
			}
		}

        if (!waitForResponse)
            return "";

		HttpWebResponse httpResponse = null;
        try
        {
            httpResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
        }
        catch (WebException ex)
        {
            Debug.Log("WebException caught, Status code: " + ex.Status + " " + ex.ToString());
            if (ex.Response != null && ((HttpWebResponse)ex.Response).StatusCode > HttpStatusCode.Accepted)
            {
                Debug.Log("HTTP problem detected! Code: " + (int)(((HttpWebResponse)ex.Response).StatusCode));
            }
            httpResponse = (HttpWebResponse)ex.Response;
        }
        catch (System.Exception ex)
        {
            Debug.Log("HttpWebRequest::GetResponse exception caught " + ex.ToString());
        }
		finally
		{
            if (httpResponse != null)
            {
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    ret = streamReader.ReadToEnd();
                }
            }
		}		
		return ret;
	}
}
