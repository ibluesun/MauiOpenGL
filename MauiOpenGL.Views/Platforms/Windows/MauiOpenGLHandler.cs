using MauiOpenGL.Views;
using Microsoft.Maui.Handlers;
using OpenTK.Graphics.ES20;

namespace MauiOpenGL.Handlers;


/// <summary>
/// This is a partial class that should exist in each platform 
/// </summary>
public partial class MauiOpenGLHandler : ViewHandler<MauiOpenGLView, WindowsOpenGLView>
{


    public static WindowsOpenTKBindingsContext WindowsOpenTKBinder { get; private set; } = new WindowsOpenTKBindingsContext();


    public static IPropertyMapper<MauiOpenGLView, MauiOpenGLHandler> PropertyMapper = new PropertyMapper<MauiOpenGLView, MauiOpenGLHandler>(ViewHandler.ViewMapper)
    {

    };

    public static CommandMapper<MauiOpenGLView, MauiOpenGLHandler> CommandMapper = new(ViewCommandMapper)
    {

    };

    public MauiOpenGLHandler() : base(PropertyMapper, CommandMapper)
    {
    }

    public MauiOpenGLHandler(IPropertyMapper mapper, CommandMapper commandMapper = null) : base(mapper, commandMapper)
    {
    }

    protected override WindowsOpenGLView CreatePlatformView()
    {
        // let OpenTK knows where are the functions :)



        var aog = new WindowsOpenGLView();

        GL.LoadBindings(WindowsOpenTKBinder);

        return aog;

    }

    protected override void ConnectHandler(WindowsOpenGLView platformView)
    {
        base.ConnectHandler(platformView);
    }

    protected override void DisconnectHandler(WindowsOpenGLView platformView)
    {
        platformView.Dispose();

        base.DisconnectHandler(platformView);
    }
}
