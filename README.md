# Simple Unity browser #

This is a simple embedded browser plugin. It is based on CefGlue, and using a background process to render web pages. 
    
  | 
------------- | -------------
![unity_small.png](https://bitbucket.org/repo/xLMGXM/images/2197541935-unity_small.png)  | ![html5test_small.png](https://bitbucket.org/repo/xLMGXM/images/3949485457-html5test_small.png)


## Basic setup ##

Import the package to Unity. There will be a few folders in *Assets/SimpleWebBrowser*. You can move/rename all of them, but in case of *Assets/SimpleWebBrowser/PluginServer* you will need to change the runtime and deployment paths in *Scripts/BrowserEngine.cs* and in *Editor/BrowserPostBuild.cs*:


```
#!c#
 public void InitPlugin(int width, int height, string sharedfilename, int port, string initialURL)
        {

            //Initialization (for now) requires a predefined path to PluginServer,
            //so change this section if you move the folder
            //Also change the path in deployment script.

#if UNITY_EDITOR_64
            string PluginServerPath = Application.dataPath + @"\SimpleWebBrowser\PluginServer\x64";
#else
#if UNITY_EDITOR_32
        string PluginServerPath = Application.dataPath + @"\SimpleWebBrowser\PluginServer\x86";


```

The package contains two demo scenes, for the inworld and canvas browser; you can use them as a reference, or just drag one of the prefabs to the scene.
 
##Settings ##

![browser_settings.png](https://bitbucket.org/repo/xLMGXM/images/2087941195-browser_settings.png)

* Width and height - width and height of the browser texture.
* Memory file and Port - in general, you can keep them random. Memory file is the name of a shared memory file, which is used to send the texture data, and port is a TCP port for the communication between browser and plugin. In case of random, memory file will be a random GUID, and a port will be between 8000 and 9000. But you can set them manually, if you want.
* Initial URL - obviously, the initial browser URL.
* UI settings - settings for the main browser controls. You can customize them the way you like, changing they appearance in editor and behaviour in BrowserUI.cs. 
* Dialog settings - same as UI, but for modal dialogs.

2D browser setup is almost the same, except the Browser2D raw image texture setting, which is the base texture for browser.

