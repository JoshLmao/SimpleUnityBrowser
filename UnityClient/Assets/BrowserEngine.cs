using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using MessageLibrary;
using SharedMemory;
using UnityEngine;

public class BrowserEngine
{
    private Socket _clientSocket;

    private SharedArray<byte> _mainTexArray;

    private System.Diagnostics.Process _pluginProcess;

    private static System.Object sPixelLock;

    public Texture2D BrowserTexture=null;
    public bool Initialized = false;

   

    

    private byte[] _bufferBytes = null;
    private long _arraySize = 0;

    #region Settings
    public int kWidth = 512;
    public int kHeight = 512;
    private string _sharedFileName;
    private int _port;
    private string _initialURL;
    #endregion



    #region Init
    public void InitPlugin(int width,int height, string sharedfilename,int port,string initialURL)
    {
        kWidth = width;
        kHeight = height;
        _sharedFileName = sharedfilename;
        _port = port;
        _initialURL = initialURL;

        if(BrowserTexture==null)
        BrowserTexture = new Texture2D(kWidth, kHeight, TextureFormat.BGRA32, false);
        sPixelLock = new object();


        string args = BuildParamsString();
        _pluginProcess = new System.Diagnostics.Process()
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo()
            {
                WorkingDirectory =
                    @"D:\work\unity\StandaloneConnector\SharedPluginServer\SharedPluginServer\bin\x64\Debug",
                FileName =
                    @"D:\work\unity\StandaloneConnector\SharedPluginServer\SharedPluginServer\bin\x64\Debug\SharedPluginServer.exe",
                Arguments = args

            }
        };



        _pluginProcess.Start();


    }

    private string BuildParamsString()
    {
        string ret = kWidth.ToString() + " " + kHeight.ToString() + " ";
        ret = ret + _initialURL + " ";
        ret = ret + _sharedFileName + " ";
        ret = ret + _port.ToString();
        return ret;
    }

    #endregion

    #region SendEvents

    public void SendNavigateEvent(string url)
    {
        GenericEvent ge = new GenericEvent()
        {
            Type = GenericEventType.Navigate,
            GenericType = MessageLibrary.EventType.Generic,
            NavigateUrl = url
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

    public void SendCharEvent(int character, KeyboardEventType type)
    {
        
        KeyboardEvent keyboardEvent = new KeyboardEvent()
        {
            Type = type,
            Key = character
        };
        EventPacket ep = new EventPacket()
        {
            Event = keyboardEvent,
            Type = MessageLibrary.EventType.Keyboard
        };

        MemoryStream mstr = new MemoryStream();
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(mstr, ep);
        byte[] b = mstr.GetBuffer();
        _clientSocket.Send(b);
    }

    public void SendMouseEvent(MouseMessage msg)
    {
       
        EventPacket ep = new EventPacket
        {
            Event = msg,
            Type = MessageLibrary.EventType.Mouse
        };

        MemoryStream mstr = new MemoryStream();
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(mstr, ep);
        byte[] b = mstr.GetBuffer();
        _clientSocket.Send(b);
       
    }

    #endregion

    

    public void UpdateTexture()
    {
        if (Initialized)
        {


            if (_bufferBytes == null)
            {
                long arraySize = _mainTexArray.Length;
                Debug.Log(arraySize);
                _bufferBytes = new byte[arraySize];
            }
            _mainTexArray.CopyTo(_bufferBytes, 0);

            lock (sPixelLock)
            {
                BrowserTexture.LoadRawTextureData(_bufferBytes);
                BrowserTexture.Apply();
            }



        }
        else
        {
            try
            {
                string processName = _pluginProcess.ProcessName;//could be InvalidOperationException
                foreach (System.Diagnostics.Process clsProcess in System.Diagnostics.Process.GetProcesses())
                    if (clsProcess.ProcessName == processName) 
                    {
                        Thread.Sleep(200); //give it some time to initialize
                        try
                        {
                            _mainTexArray = new SharedArray<byte>(_sharedFileName);
                            //Connect
                            IPAddress ip = IPAddress.Parse("127.0.0.1");
                            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            _clientSocket.Connect(new IPEndPoint(ip, _port));
                            Initialized = true;
                        }
                        catch (Exception)
                        {
                            //SharedMem and TCP exceptions
                            
                        }



                    }
            }
            catch (Exception)
            {
                
                //InvalidOperationException
            }
            
        }
    }

    public void Shutdown()
    {
        SendShutdownEvent();
        _clientSocket.Close();
    }
}