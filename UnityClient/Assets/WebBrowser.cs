using System;
using UnityEngine;
using System.Collections;
//using System.Diagnostics;
using MessageLibrary;
using UnityEngine.UI;

public class WebBrowser : MonoBehaviour
{

    #region General

    [Header("General settings")]
    public int Width = 1024;

    public int Height = 768;

    public string MemoryFile = "MainSharedMem";

    public bool RandomMemoryFile;

    [Range(8000f,9000f)]
    public int Port=8885;

    public bool RandomPort;

    public string InitialURL = "http://www.google.com";

   
    #endregion


    [Header("UI settings")]
    public BrowserUI mainUIPanel;

    public bool KeepUIVisible = false;

    private Material _mainMaterial;

    



    private BrowserEngine _mainEngine;

    

    private bool _focused = false;
    

    private int posX = 0;
    private int posY = 0;

    private Camera _mainCamera;

    void Awake()
    {
        _mainEngine=new BrowserEngine();

        if (RandomMemoryFile)
        {
            Guid memid = Guid.NewGuid();
            MemoryFile = memid.ToString();
        }
        if (RandomPort)
        {
            System.Random r = new System.Random();
            Port = 8880 + r.Next(10);
        }
       
        _mainEngine.InitPlugin(Width,Height,MemoryFile,Port,InitialURL);
    }

    // Use this for initialization
    void Start ()
    {
        _mainMaterial = GetComponent<MeshRenderer>().material;
        _mainMaterial.SetTexture("_MainTex",_mainEngine.BrowserTexture);
        _mainMaterial.SetTextureScale("_MainTex", new Vector2(-1, 1));
        _mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        mainUIPanel.MainCanvas.worldCamera = _mainCamera;
       // _mainInput = MainUrlInput.GetComponent<Input>();
        mainUIPanel.KeepUIVisible = KeepUIVisible;
        if(!KeepUIVisible)
            mainUIPanel.Hide();
    }

    #region UI

    public void OnNavigate()
    {
       // MainUrlInput.isFocused
        _mainEngine.SendNavigateEvent(mainUIPanel.UrlField.text);
    }
    #endregion


    #region Events
    void OnMouseEnter()
    {
      //  if (!_focused)
       // {
            _focused = true;
        //}
        Debug.Log("________ENTER");
      mainUIPanel.Show();
    }

    void OnMouseExit()
    {
        Debug.Log("________Leave");
        _focused = false;
        mainUIPanel.Hide();
    }

    void OnMouseDown()
    {
        
        if (_mainEngine.Initialized)
        {
            Vector2 pixelUV = GetScreenCoords();

            if (pixelUV.x > 0)
            {
                SendMouseButtonEvent((int)pixelUV.x,(int)pixelUV.y,MouseButton.Left, MouseEventType.ButtonDown);
               
            }
        }

    }

    


    void OnMouseUp()
    {
        if (_mainEngine.Initialized)
        {
            Vector2 pixelUV = GetScreenCoords();

            if (pixelUV.x > 0)
            {
                SendMouseButtonEvent((int)pixelUV.x, (int)pixelUV.y, MouseButton.Left, MouseEventType.ButtonUp);
            }
        }
    }

    void OnMouseOver()
    {
        if (_mainEngine.Initialized)
        {
            Vector2 pixelUV = GetScreenCoords();

            if (pixelUV.x > 0)
            {
                int px = (int) pixelUV.x;
                int py = (int) pixelUV.y;

                ProcessScrollInput(px, py);

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

                //check other buttons...
                if(Input.GetMouseButtonDown(1))
                    SendMouseButtonEvent(px,py,MouseButton.Right, MouseEventType.ButtonDown);
                if (Input.GetMouseButtonUp(1))
                    SendMouseButtonEvent(px, py, MouseButton.Right, MouseEventType.ButtonUp);
                if (Input.GetMouseButtonDown(2))
                    SendMouseButtonEvent(px, py, MouseButton.Middle, MouseEventType.ButtonDown);
                if (Input.GetMouseButtonUp(2))
                    SendMouseButtonEvent(px, py, MouseButton.Middle, MouseEventType.ButtonUp);
            }
        }

        // Debug.Log(pixelUV);
  }

    #endregion
    #region Helpers

    private Vector2 GetScreenCoords()
    {
        RaycastHit hit;
        if (
            !Physics.Raycast(
                _mainCamera.ScreenPointToRay(Input.mousePosition), out hit))
            return new Vector2(-1f, -1f);
        Texture tex = _mainMaterial.mainTexture;


        Vector2 pixelUV = hit.textureCoord;
        pixelUV.x = (1 - pixelUV.x) * tex.width;
        pixelUV.y *= tex.height;


        return pixelUV;
    }

    private void SendMouseButtonEvent(int x,int y,MouseButton btn,MouseEventType type)
    {
        MouseMessage msg = new MouseMessage
        {
            Type = type,
            X = x,
            Y = y,
            GenericType = MessageLibrary.EventType.Mouse,
            // Delta = e.Delta,
            Button = btn
        };
        _mainEngine.SendMouseEvent(msg);
    }

    private void ProcessScrollInput(int px, int py)
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        scroll = scroll*_mainEngine.BrowserTexture.height;

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
    }
    #endregion

    // Update is called once per frame
    void Update ()
    {

        _mainEngine.UpdateTexture();

	    if (_focused&&!mainUIPanel.UrlField.isFocused) //keys
	    {
	        foreach (char c in Input.inputString)
	        {
                Debug.Log(Input.inputString);
	            
	                _mainEngine.SendCharEvent((int) c, KeyboardEventType.CharKey);
	           
	            
	        }
	        if (Input.GetKeyDown(KeyCode.Backspace))
	            _mainEngine.SendCharEvent(8, KeyboardEventType.Down);
            if (Input.GetKeyUp(KeyCode.Backspace))
                _mainEngine.SendCharEvent(8, KeyboardEventType.Up);
            
	        //if (Input.GetKeyUp(KeyCode.Backspace))
             //   _mainEngine.SendCharEvent(8, KeyboardEventType.Up);



        }
    }

    void OnDisable()
    {
       _mainEngine.Shutdown();
    }

    
}