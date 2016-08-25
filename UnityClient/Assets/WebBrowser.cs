using System;
using UnityEngine;
using System.Collections;
//using System.Diagnostics;
using System.IO;
using SharedMemory;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using MessageLibrary;

public class WebBrowser : MonoBehaviour
{
   

    

    private Material _mainMaterial;

    



    private BrowserEngine _mainEngine;

    

    private bool _focused = false;
    

    private int posX = 0;
    private int posY = 0;

    void Awake()
    {
        _mainEngine=new BrowserEngine();
       
        _mainEngine.InitPlugin();
    }

    // Use this for initialization
    void Start ()
    {
        _mainMaterial = GetComponent<MeshRenderer>().material;
        _mainMaterial.SetTexture("_MainTex",_mainEngine.BrowserTexture);
        _mainMaterial.SetTextureScale("_MainTex", new Vector2(-1, 1));
    }

    void OnMouseEnter()
    {
      //  if (!_focused)
       // {
            _focused = true;
        //}
    }

    void OnMouseLeave()
    {
        _focused = false;
    }

    void OnMouseDown()
    {
        if (_mainEngine.Initialized)
        {
            RaycastHit hit;
            if (
                !Physics.Raycast(
                    GameObject.Find("Main Camera").GetComponent<Camera>().ScreenPointToRay(Input.mousePosition), out hit))
                return;
            Texture tex = _mainMaterial.mainTexture;
            Debug.Log(hit.textureCoord);

            Vector2 pixelUV = hit.textureCoord;
            pixelUV.x = (1 - pixelUV.x)*tex.width;
            pixelUV.y *= tex.height;

            Debug.Log(pixelUV);

            MouseMessage msg = new MouseMessage
            {
                Type = MouseEventType.ButtonDown,
                X = (int) pixelUV.x,
                Y = (int) pixelUV.y,
                GenericType = MessageLibrary.EventType.Mouse,
                Button = MouseButton.Left
            };
            if (Input.GetMouseButtonDown(0))
                msg.Button = MouseButton.Left;
            if (Input.GetMouseButtonDown(1))
                msg.Button = MouseButton.Left;
            if (Input.GetMouseButtonDown(2))
                msg.Button = MouseButton.Left;

            _mainEngine.SendMouseEvent(msg);
        }

    }

    void OnMouseUp()
    {
        if (_mainEngine.Initialized)
        {
            RaycastHit hit;
            if (
                !Physics.Raycast(
                    GameObject.Find("Main Camera").GetComponent<Camera>().ScreenPointToRay(Input.mousePosition), out hit))
                return;
            Texture tex = _mainMaterial.mainTexture;
            Debug.Log(hit.textureCoord);

            Vector2 pixelUV = hit.textureCoord;
            pixelUV.x = (1 - pixelUV.x)*tex.width;
            pixelUV.y *= tex.height;

            Debug.Log(pixelUV);

            MouseMessage msg = new MouseMessage
            {
                Type = MouseEventType.ButtonUp,
                X = (int) pixelUV.x,
                Y = (int) pixelUV.y,
                GenericType = MessageLibrary.EventType.Mouse,
                Button = MouseButton.Left
            };
            if (Input.GetMouseButtonUp(0))
                msg.Button = MouseButton.Left;
            if (Input.GetMouseButtonUp(1))
                msg.Button = MouseButton.Right;
            if (Input.GetMouseButtonUp(1))
                msg.Button = MouseButton.Middle;

            _mainEngine.SendMouseEvent(msg);
        }
    }

    void OnMouseOver()
    {
        if (_mainEngine.Initialized)
        {
            RaycastHit hit;
            if (
                !Physics.Raycast(
                    GameObject.Find("Main Camera").GetComponent<Camera>().ScreenPointToRay(Input.mousePosition), out hit))
                return;
            Texture tex = _mainMaterial.mainTexture;
            // Debug.Log(hit.textureCoord);

            Vector2 pixelUV = hit.textureCoord;
            pixelUV.x = (1 - pixelUV.x)*tex.width;
            pixelUV.y *= tex.height;

            int px = (int) pixelUV.x;
            int py = (int) pixelUV.y;

            float scroll = Input.GetAxis("Mouse ScrollWheel");

            scroll = scroll*tex.height;

            int scInt = (int) scroll;

            if (scInt != 0)
            {
                MouseMessage msg = new MouseMessage
                {
                    Type = MouseEventType.Wheel,
                    X = px,
                    Y = py,
                    GenericType = MessageLibrary.EventType.Mouse,
                    Delta = scInt,
                    Button = MouseButton.None
                };

                if (Input.GetMouseButton(0))
                    msg.Button = MouseButton.Left;
                if (Input.GetMouseButton(1))
                    msg.Button = MouseButton.Right;
                if (Input.GetMouseButton(1))
                    msg.Button = MouseButton.Middle;

                _mainEngine.SendMouseEvent(msg);
            }

            if (posX != px || posY != py)
            {
                MouseMessage msg = new MouseMessage
                {
                    Type = MouseEventType.Move,
                    X = px,
                    Y = py,
                    GenericType = MessageLibrary.EventType.Mouse,
                    // Delta = e.Delta,
                    Button = MouseButton.None
                };

                if (Input.GetMouseButton(0))
                    msg.Button = MouseButton.Left;
                if (Input.GetMouseButton(1))
                    msg.Button = MouseButton.Right;
                if (Input.GetMouseButton(1))
                    msg.Button = MouseButton.Middle;

                posX = px;
                posY = py;
                _mainEngine.SendMouseEvent(msg);
            }
        }

        // Debug.Log(pixelUV);
  }

    
	
	// Update is called once per frame
	void Update ()
    {

        _mainEngine.UpdateTexture();
    }

    void OnDisable()
    {
       _mainEngine.Shutdown();
    }

    
}

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
            foreach (System.Diagnostics.Process clsProcess in System.Diagnostics.Process.GetProcesses())
                if (clsProcess.ProcessName == _pluginProcess.ProcessName)
                {
                    Thread.Sleep(100); //give some time to initialize
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

    public void Shutdown()
    {
        SendShutdownEvent();
        _clientSocket.Close();
    }
}
