//using OpenTK.Windowing.Common;
//using OpenTK.Wpf.Interop;
//using Silk.NET.DXGI;



using Microsoft.UI.Xaml.Controls;

namespace MauiOpenGL.Views
{
    // All the code in this file is only included on Windows.
    public class WindowsOpenGLView : Microsoft.UI.Xaml.Controls.Grid

    // : Microsoft.UI.Xaml.Controls.ListBox
    {


        AngleSwapChainPanel MainAngleSwapChainPanel = new AngleSwapChainPanel();

        public WindowsOpenGLView() 
        {
            
            this.Children.Add(MainAngleSwapChainPanel);

        }



        public void Dispose()
        {

        }
    }
}