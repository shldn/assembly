using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using WSAUnityHRMStub;

/*
 * Mode A uses polling to monitor the plugin and retrieve new values from the plugin. 
 */
// heart image source: https://commons.wikimedia.org/wiki/File:Heart_icon_red_hollow.svg

public class ModeAController : MonoBehaviour
{
    private const float HEART_SCALE_MIN = 0.25f;
    private const float HEART_SCALE_MAX = 0.26f;
    private const float HEART_SCALE_RANGE = HEART_SCALE_MAX - HEART_SCALE_MIN;
     
    private HRMPlugin _hrmPlugin;
    private HeartRateServiceDevice _selectedDevice;

    private int _lastHRValue;
    private float _beatStartTime;
    private float _beatCompleteTime;
    private float _rrDuration; // time in seconds for one heart beat

    // optional polling instead of Queue/Action technique for connecting to HRM device
    private int _scanPoll = 0;
   
    private GameObject _heartGO;
    private Text _hrmStatusLabel;
    private Button _scanBtn;
    private GameObject _hrmDropDownGO;
    private Dropdown _hrmDropdown;
    private Button _connectBtn;
    private Button _disconnectBtn;
    private Text _BPMDisplay;

    private string _hrmStatusMsgStart = "Scan for Paired HRM Devices";
    private string _hrmStatusMsgScanning = "Scanning...";
    private string _hrmStatusMsgScanCompleteSuccess = "Select an HRM Device";
    private string _hrmStatusMsgScanNoDevices = "Pair your HRM in Settings then Try Again";
    private string _hrmStatusMsgDeviceSelected = "Click Connect";
    private string _hrmStatusMsgConnecting = "Connecting...";
    private string _hrmStatusMsgConnected = "Connected";
    private string _dropdownBlankEntryLabel = "None";
    private string _dropdownCaptionStart = "HRM Device List";
    private string _dropdownCaptionSelect = "Select an HRM Device";
    private List<Text> _values;
    private GameObject _canvasValueDisplay;

    void Start()
    {
        Application.targetFrameRate = 60;

        resetValues();

        _heartGO = GameObject.Find("Heart").gameObject;

        GameObject tCanvasGO = GameObject.Find("Canvas");
        
        _hrmStatusLabel = tCanvasGO.transform.Find("HRMStatus").GetComponent<Text>();
        _hrmStatusLabel.text = _hrmStatusMsgStart;

        _scanBtn = tCanvasGO.transform.Find("ScanButton").GetComponent<Button>();
        _scanBtn.onClick.AddListener(onScanClicked);
        _scanBtn.enabled = true;

        _hrmDropDownGO = tCanvasGO.transform.Find("Dropdown").gameObject;
        _hrmDropdown = _hrmDropDownGO.GetComponent<Dropdown>();
        _hrmDropdown.captionText.text = _dropdownCaptionStart;
        _hrmDropdown.onValueChanged.AddListener(onDropDownSelected);
        _hrmDropdown.enabled = false;

        _connectBtn = tCanvasGO.transform.Find("ConnectButton").GetComponent<Button>();
        _connectBtn.onClick.AddListener(onConnectClicked);
        _connectBtn.enabled = false;

        _disconnectBtn = tCanvasGO.transform.Find("DisconnectButton").GetComponent<Button>();
        _disconnectBtn.onClick.AddListener(onDisconnectClicked);
        _disconnectBtn.enabled = false;

        _BPMDisplay = tCanvasGO.transform.Find("BPMDisplay").GetComponent<Text>();

        _hrmPlugin = new HRMPlugin();
    }

    private void resetAll()
    {
        resetValues();

        _hrmStatusLabel.text = _hrmStatusMsgDeviceSelected;
        _BPMDisplay.text = "0";
    }

    private void resetValues()
    {
        _lastHRValue = 0;
        _beatStartTime = 0;
        _beatCompleteTime = 0;
        _rrDuration = 0;
    }

    private void onScanClicked()
    {
        _hrmStatusLabel.text = _hrmStatusMsgScanning;
        
        _hrmPlugin.disconnectService();
        resetAll();

        // Mode A: scan with polling
        _hrmPlugin.scan();
        _scanPoll = 1;
    }

    public void onScanComplete()
    {
        if ( _hrmPlugin.hrsDevices.Count == 0 )
        {
            _hrmStatusLabel.text = _hrmStatusMsgScanNoDevices;
        }
        else if ( _hrmPlugin.hrsDevices.Count > 0 )
        {
            _hrmStatusLabel.text = _hrmStatusMsgScanCompleteSuccess;

            addDropDownEntries();
        }
    }

    // only reach here if there are 1 or more devices found
    private void addDropDownEntries()
    {
        _hrmDropdown.ClearOptions();
        List<string> tDeviceNames = new List<string>();

        if (_hrmPlugin.hrsDevices.Count == 1)
        {
            tDeviceNames.Add(_hrmPlugin.hrsDevices[0].name);
            
            _hrmDropdown.AddOptions(tDeviceNames);
            
            _hrmDropdown.enabled = false;

            onDropDownSelected(1);
        }
        else 
        {
            // to prevent auto-selection of the first item in the list add a blank item at the top of the list
            Dropdown.OptionData tLabelEmpty = new Dropdown.OptionData();
            tLabelEmpty.text = _dropdownBlankEntryLabel;
            _hrmDropdown.options.Add(tLabelEmpty);
            
            foreach (HeartRateServiceDevice tDevice in _hrmPlugin.hrsDevices)
            {
                tDeviceNames.Add(tDevice.name);
            }
            _hrmDropdown.AddOptions(tDeviceNames);

            _hrmDropdown.captionText.text = _dropdownCaptionSelect;

            _hrmDropdown.enabled = true;
        }
    }

    private void onDropDownSelected(int pSelectedIndex)
    {
        _connectBtn.enabled = true;

        _hrmStatusLabel.text = _hrmStatusMsgDeviceSelected;
    }

    private void onConnectClicked()
    {
        int tSelectedIndex;

        // when there was only one HRM detected we didn't push a spaceholder item into the dropdown,
        // so no need to adjust the index reference
        if (_hrmDropdown.options.Count == 1)
        {
            tSelectedIndex = _hrmDropDownGO.GetComponent<Dropdown>().value;
        }
        else
        { 
            tSelectedIndex = _hrmDropDownGO.GetComponent<Dropdown>().value - 1;
        }

        _selectedDevice = _hrmPlugin.hrsDevices[tSelectedIndex];

        resetValues();

        _hrmStatusLabel.text = _hrmStatusMsgConnecting;
        _BPMDisplay.text = "0";

        _connectBtn.enabled = false;
        _disconnectBtn.enabled = true;

        // Mode A: connect with polling technique
        _hrmPlugin.initializeService( _selectedDevice );

        // default is 100 saved byte arrays. 
        _hrmPlugin.ReceivedMeasurementDataStorageMax = 20;
    }

    private void onDisconnectClicked()
    {
        resetAll();
        
        _connectBtn.enabled = true;
        _disconnectBtn.enabled = false;

        _hrmPlugin.disconnectService();        
    }

    private void processHRValues()
    {
        if (_hrmPlugin.hrms.Count <= 0)
            return;

        // returns ushort if there's a new entry, otherwise returns 0
        // pass true (default) to reset status of *new* available data in plugin
        ushort tHRM = _hrmPlugin.getLastHRM(true);
        if (tHRM != 0 )
        {
            _lastHRValue = tHRM;
            
            _hrmStatusLabel.text = _hrmStatusMsgConnected;
            _BPMDisplay.text = _lastHRValue.ToString();            
        }
        byte[] tBytes = _hrmPlugin.receivedMeasurementData[ _hrmPlugin.receivedMeasurementData.Count - 1 ];
        int tBytesLength = tBytes.Length;
        _hrmStatusLabel.text = tBytes.Length.ToString();

        string tDebug = _hrmPlugin.receivedMeasurementData.Count + " measurements: ";
        for ( int i = 0; i<tBytesLength; i++ )
        {
            tDebug += tBytes[i] + "-";
        }
        Debug.Log(tDebug);

        _hrmStatusLabel.text = tDebug;
    }

    void Update()
    {
        // Mode A: poll for completion of scanning 
        if ( _scanPoll == 1 && _hrmPlugin.isScanComplete )
        {
            _scanPoll = 0;

            onScanComplete();

            return;
        }

        // Mode A: poll for new HR values
        if ( !_hrmPlugin.isServiceInitialized )
        { 
            return;
        }

        if( _hrmPlugin.getHasNewHRValue() )
        {
            processHRValues();
        }

        // animate heart
        if (_lastHRValue != 0)
        {
            float tBeatPhase = 0;
            if (Time.time >= _beatCompleteTime)
            {
                _rrDuration = 60f / _lastHRValue;
                _beatStartTime = Time.time;
                _beatCompleteTime = Time.time + _rrDuration;
                tBeatPhase = 0;
            }
            else
            {
                tBeatPhase = (Time.time - _beatStartTime) / _rrDuration;
            }

            float tBeatPhaseSmoothed = Mathf.SmoothStep(0, 1, tBeatPhase);
            float tScale = HEART_SCALE_MAX - (HEART_SCALE_RANGE * tBeatPhaseSmoothed);

            Mathf.Clamp(tScale, HEART_SCALE_MIN, HEART_SCALE_MAX);
            _heartGO.transform.localScale = new Vector3(tScale, tScale, tScale);
        }
    }
}
