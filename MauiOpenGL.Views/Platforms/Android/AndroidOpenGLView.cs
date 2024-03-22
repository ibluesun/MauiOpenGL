using Android.Content;
using Android.Opengl;
using Android.Runtime;
using Android.Util;
using Java.Interop;
using Javax.Microedition.Khronos.Egl;
using Javax.Microedition.Khronos.Opengles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.ES20;

namespace MauiOpenGL.Views;



/// <summary>
/// The native view that will be used in the Android  .. only included in the android part.
/// </summary>
public class AndroidOpenGLView : GLSurfaceView
{

    public AndroidOpenGLView(Context context) : base(context)
    {
        
        SetEGLContextClientVersion(2);

        this.PreserveEGLContextOnPause = false;






        SetRenderer(new AndroidOpenGLRenderer());
        RenderMode = Rendermode.WhenDirty;

    }




}
