# How the "third_party" folder works

This folder contains the built binaries for cef_64 and cef_86, the folder with required files for Widevine and the SharedMemory.dll.

### Cef_X

The CEF binaries are needed to be next to the running .exe to allow it to run. There are Post-build event commands to copy the 
cef_64 folder to the Debug directories of the "TestServer" and "SharedPluginServer" projects.

### WidevineCdm

Just like the Cef_64 folder, there is also a post-build event command to copy the WidevineCdm folder to the Debug directory. This is 
setup so the changes in this folder will be applied everywhere
