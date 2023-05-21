using Android.Opengl;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;



namespace MauiOpenGL.Views;

public class AndroidOpenTKBindingsContext : IBindingsContext
{


    [DllImport("/system/lib64/libEGL.so", EntryPoint = "eglGetProcAddress")]
    public static extern IntPtr EglGetProcAddress(string procName);


    public IntPtr GetProcAddress(string procName)
    {
        var glfunc = EglGetProcAddress(procName);

        return glfunc;
       
    }
}
