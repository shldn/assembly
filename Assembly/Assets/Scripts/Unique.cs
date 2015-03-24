using UnityEngine;
using System.Collections;
using System.Net.NetworkInformation;

public class Unique {

    public static string ID {
        get{
#if UNITY_STANDALONE
            return GetMacAddress();
#elif UNITY_ANDRIOD
            return GetAndroidID();
#endif
        }
    }

    private static string GetMacAddress()
    {
        IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
        NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

        if (nics == null || nics.Length < 1)
        {
            Debug.LogError("No network interfaces found.");
            return "";
        }

        foreach (NetworkInterface adapter in nics)
        {
            IPInterfaceProperties properties = adapter.GetIPProperties();
            PhysicalAddress address = adapter.GetPhysicalAddress();
            byte[] bytes = address.GetAddressBytes();
            return address.ToString();
        }
        return "";
    }

#if UNITY_ANDRIOD
    private static string GetAndroidID()
    {
        AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver");
        AndroidJavaClass secure = new AndroidJavaClass("android.provider.Settings$Secure");
        return secure.CallStatic<string>("getString", contentResolver, "android_id");
    }
#endif
}
