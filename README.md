# Simple Unity browser #

This is a simple embedded browser plugin. It is based on CefGlue, and using a background process to render web pages. 
    
  | 
------------- | -------------
![unity_small.png](https://bitbucket.org/repo/xLMGXM/images/2197541935-unity_small.png)  | ![html5test_small.png](https://bitbucket.org/repo/xLMGXM/images/3949485457-html5test_small.png)


## Basic setup ##

Import the package to Unity. There will be a few folders in Assets/SimpleWebBrowser. You can move/rename all of them, but in case of Assets/SimpleWebBrowser/PluginServer you will need to change the runtime and deployment paths in Scripts/BrowserEngine.cs and in Editor/BrowserPostBuild.cs:


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
#else


        //HACK
        string AssemblyPath=System.Reflection.Assembly.GetExecutingAssembly().Location;
        //log this for error handling
        Debug.Log("Assembly path:"+AssemblyPath);

        AssemblyPath = Path.GetDirectoryName(AssemblyPath); //Managed
      
        AssemblyPath = Directory.GetParent(AssemblyPath).FullName; //<project>_Data
        AssemblyPath = Directory.GetParent(AssemblyPath).FullName;//required

        string PluginServerPath=AssemblyPath+@"\PluginServer";
#endif
#endif

```

