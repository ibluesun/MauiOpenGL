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


# Windows Handler

**Maui** uses WinUI3 inside its handler .. so to be able to use OpenGL ES you will have to rely on  Google **ANGLE**  (*Almost Native Graphics Layer Engine**) project.

however the dll that is being used (as far as I know from this issue https://github.com/mono/SkiaSharp/issues/1893  is actually not targetting WinUI3 yet  .. but only targetting the **WinRT** API.

so .. thanks to @levinli303 https://github.com/levinli303  and his patched dll .. found in https://github.com/mono/SkiaSharp/issues/1893#issuecomment-1805076715  

and also can be found in his repo https://github.com/microsoft/vcpkg/compare/master...levinli303:vcpkg:angle-winui 

also thanks to **SkiaSharp** project  (I used their code in creating the Windows SwapChainPanel) 

the windows part can be shown beside the android one on the below snapshot



## Future 

Naturally  ..  Apple and IOS  should be the focus on the future  ..  but I don't have such devices for now.

## Finally 

This repo is a proof of concept for using OpenTK in Maui  ..  the code is **Spaghetti**  and it is not a production thing by anymean  .. so take it with (two or three) grain of salt :D 

Thank you,

	Sadek


![alt text][logo]

[logo]: MauiOpenGL.png "Maui Open GL"