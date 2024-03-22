using Android.Content;
using Android.Runtime;
using Android.Util;
using Java.Interop;
using OpenTK.Graphics.Egl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Android.Opengl;
using Javax.Microedition.Khronos.Opengles;
using OpenTK.Graphics.ES20;



namespace MauiOpenGL.Views;


public class AndroidOpenGLRenderer : Java.Lang.Object, GLSurfaceView.IRenderer
{
    EGLContext _CurrentContext;
    EGLDisplay _CurrentDisplay;


    IntPtr _AndroidContextNativeHandle;

    public EGLContext CurrentContext => _CurrentContext;
    public EGLDisplay CurrentDisplay => _CurrentDisplay;
    public IntPtr AndroidContextNativeHandle => _AndroidContextNativeHandle;

    public string EGL_VERSION { get; private set; }

    LightGraphicsEngine.GradientBackground grb = new LightGraphicsEngine.GradientBackground();


    public void OnSurfaceCreated(IGL10 gl, Javax.Microedition.Khronos.Egl.EGLConfig config)
    {

        _CurrentContext = EGL14.EglGetCurrentContext();
        _CurrentDisplay = EGL14.EglGetCurrentDisplay();


        EGL_VERSION = EGL14.EglQueryString(_CurrentDisplay, EGL14.EglVersion);

        _AndroidContextNativeHandle = new IntPtr(_CurrentContext.NativeHandle);


        //GLES20.GlClearColor(1f, 0f, 0f, 1f);


        GL.ClearColor(0, 0, 0, 0);

    }


    public void OnSurfaceChanged(IGL10 gl, int width, int height)
    {
        //GLES20.GlViewport(0, 0, width, height);
        GL.Viewport(0, 0, width, height);
    }

    public void OnDrawFrame(IGL10 gl)
    {

        //GLES20.GlClear(GLES20.GlColorBufferBit | GLES20.GlDepthBufferBit);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        grb.Render();
    }
}

