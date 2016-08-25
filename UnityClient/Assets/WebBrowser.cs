using System;
using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.IO;
using SharedMemory;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using MessageLibrary;

public class WebBrowser : MonoBehaviour
{
    private Socket _clientSocket;

    private SharedArray<byte> _mainTexArray;

    public Texture2D BrowserTexture;

    private Material _mainMaterial;

    private const int kWidth = 512;
    private const int kHeight = 512;

    public bool Initialized=false;

    private static System.Object sPixelLock;

    private Process _pluginProcess;

    void Awake()
    {
        BrowserTexture = new Texture2D(kWidth, kHeight, TextureFormat.RGBA32, false);
        sPixelLock = new object();
        InitPlugin();
    }

    // Use this for initialization
    void Start ()
    {
        _mainMaterial = GetComponent<MeshRenderer>().material;
        _mainMaterial.SetTexture("_MainTex",BrowserTexture);
        _mainMaterial.SetTextureScale("_MainTex", new Vector2(-1, 1));
    }

    public void InitPlugin()
    {
        string args = "512 512";
        _pluginProcess = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                WorkingDirectory =
                    @"D:\work\unity\StandaloneConnector\SharedPluginServer\SharedPluginServer\bin\x64\Debug",
                FileName =
                    @"D:\work\unity\StandaloneConnector\SharedPluginServer\SharedPluginServer\bin\x64\Debug\SharedPluginServer.exe",
                Arguments = args

            }
        };

       
       
        _pluginProcess.Start();
       // string proc_name = pluginProcess.ProcessName;
       // UnityEngine.Debug.Log("___NAME:" + proc_name);

        //Thread.Sleep(10000);
        // while (!Process.GetProcesses().Any(p => p.Name == myName)) { Thread.Sleep(100); }

        /*bool found_proc = false;
        while (!found_proc)
        {
            foreach (Process clsProcess in Process.GetProcesses())
                if (clsProcess.ProcessName == proc_name)
                    found_proc = true;

            Thread.Sleep(1000);
        }*/

       // _mainTexArray=new SharedArray<byte>("MainSharedMem");

        //Connect
        //IPAddress ip = IPAddress.Parse("127.0.0.1");
        //_clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //_clientSocket.Connect(new IPEndPoint(ip, 8885));
       // Initialized = true;
    }
	
	// Update is called once per frame
	void Update ()
    {
	    if (Initialized)
	    {
	        byte[] read = new byte[_mainTexArray.Length];
	        _mainTexArray.CopyTo(read, 0);
	        lock (sPixelLock)
	        {
                BrowserTexture.LoadRawTextureData(read);
                BrowserTexture.Apply();
	        }
	    }
	    else
	    {
            foreach (Process clsProcess in Process.GetProcesses())
                if (clsProcess.ProcessName == _pluginProcess.ProcessName)
                {
                    try
                    {
                        _mainTexArray = new SharedArray<byte>("MainSharedMem");
                        //Connect
                        IPAddress ip = IPAddress.Parse("127.0.0.1");
                        _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        _clientSocket.Connect(new IPEndPoint(ip, 8885));
                        Initialized = true;
                    }
                    catch (Exception)
                    {
                        
                       // throw;
                    }
                     

                   
                }
        }

    }

    void OnDisable()
    {
        SendShutdownEvent();
        _clientSocket.Close();
    }

    #region Internals
    public void SendShutdownEvent()
    {
        GenericEvent ge = new GenericEvent()
        {
            Type = GenericEventType.Shutdown,
            GenericType = MessageLibrary.EventType.Generic
        };

        EventPacket ep = new EventPacket()
        {
            Event = ge,
            Type = MessageLibrary.EventType.Generic
        };

        MemoryStream mstr = new MemoryStream();
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(mstr, ep);
        byte[] b = mstr.GetBuffer();
        _clientSocket.Send(b);

    }

    #endregion
}
