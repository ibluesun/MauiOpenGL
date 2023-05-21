
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using MauiOpenGL.Views;
using Microsoft.Maui.Handlers;
using OpenTK.Graphics.ES20;




namespace MauiOpenGL.Handlers;


/// <summary>
/// This is a partial class that should exist in each platform 
/// </summary>
public partial class MauiOpenGLHandler : ViewHandler<MauiOpenGLView, AndroidOpenGLView>
{

    public static AndroidOpenTKBindingsContext OpenTKBinder { get; private set; } = new AndroidOpenTKBindingsContext();


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

    protected override AndroidOpenGLView CreatePlatformView()
    {
        // let OpenTK knows where are the functions :)

        GL.LoadBindings(OpenTKBinder);


        return new AndroidOpenGLView(Context);
    }

    protected override void ConnectHandler(AndroidOpenGLView platformView)
    {
        base.ConnectHandler(platformView);
    }

    protected override void DisconnectHandler(AndroidOpenGLView platformView)
    {
        platformView.Dispose();

        base.DisconnectHandler(platformView);
    }
}
