# About MauiOpenGL Project

This Project is for trying out using OpenTK in MAUI App

The first implemented part  is a MAUI Handler  to a native  inherited Android GLSurfaceView 


To use OpenTK: 

- First you need to get it from its corresponding nuget package.
- Second a binding of its functions should be done  .. please have a look inside  `AndroidOpenTKBindingContext`
- In the MauiOpenGLHandler  in the `CreatePlatformView`  .. the `GL.LoadBindings(OpenTKBinder);`  were called  .. then the native view of android were created
- Rendering occurs in the `AndroidOpenGLRenderer`  ..  in this location  you can use  the Android GLES20  class or the OpenTK  GL  class .. the two apis will work.


please note that binding to the functions .. I needed to use P/Invoke       `[DllImport("/system/lib64/libEGL.so", EntryPoint = "eglGetProcAddress")]` in the implemented class.

The folder of  **/system/lib64/libEGL.so**   was the one succeeded in the emulator and Xaiomi phone.   but I can't gaurantuee it will work on the other devices.

## Note

please check the MauiProgram in the app to make sure that you've included your new created view in the maui handlers

```
.ConfigureMauiHandlers(handlers =>
{
	handlers.AddHandler(typeof(MauiOpenGLView), typeof(MauiOpenGLHandler));
})
```


## Future 

I am intending to try the same approach with a windows native view also  .. then  .. maybe getting miniMac and iPhone to try Angle on Metal  or  whatever it is :).

# Thank you

![alt text][logo]

[logo]: MauiOpenGL.png "Logo Title Text 2"