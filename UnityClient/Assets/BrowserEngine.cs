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

    public Texture2D BrowserTexture;
    public bool Initialized = false;

    private long _arraySize = 0;

    public const int kWidth = 512;
    public const int kHeight = 512;

    private byte[] _bufferBytes = null;

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
        // if (_MouseDone)
        //_controlMem.Write(ref msg,0);
        // _MouseDone = false;
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
        //  MessageBox.Show(_sendEvents.Count.ToString());
    }

    public void InitPlugin()
    {
        BrowserTexture = new Texture2D(kWidth, kHeight, TextureFormat.RGBA32, false);
        sPixelLock = new object();
        string args = "512 512";
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
                        Thread.Sleep(100); //give it some time to initialize
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