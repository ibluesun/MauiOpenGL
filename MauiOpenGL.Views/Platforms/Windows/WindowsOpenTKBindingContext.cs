using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;



namespace MauiOpenGL.Views;

public class WindowsOpenTKBindingsContext : IBindingsContext
{


    [DllImport("libEGL.dll", EntryPoint = "eglGetProcAddress")]
    public static extern IntPtr EglGetProcAddress(string procName);


    public IntPtr GetProcAddress(string procName)
    {
        var glfunc = EglGetProcAddress(procName);

        return glfunc;
       
    }
}
