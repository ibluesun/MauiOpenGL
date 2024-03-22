

using System;
using System.Threading.Tasks;

using Microsoft.UI.Xaml;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using OpenTK.Graphics.Egl;
using OpenTK.Windowing.Common;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Core;
using WinRT;
using Rect = Windows.Foundation.Rect;
using ThreadPool = Windows.System.Threading.ThreadPool;
using Visibility = Microsoft.UI.Xaml.Visibility;


using OpenTK.Graphics.ES20;
using Microsoft.UI.Dispatching;
using System.Runtime.InteropServices;




namespace MauiOpenGL.Views
{
    public class AngleSwapChainPanel : SwapChainPanel
    {
        private static readonly DependencyProperty ProxyVisibilityProperty =
            DependencyProperty.Register(
                "ProxyVisibility",
                typeof(Visibility),
                typeof(AngleSwapChainPanel),
                new PropertyMetadata(Visibility.Visible, OnVisibilityChanged));

        private static readonly bool designMode = Microsoft.Maui.Controls.DesignMode.IsDesignModeEnabled;

        private readonly object locker = new object();

        private bool isVisible = true;
        private bool isLoaded = false;

        //private GlesContext glesContext;

        private IAsyncAction renderLoopWorker;
        private IAsyncAction renderOnceWorker;

        private bool enableRenderLoop;

        private double lastCompositionScaleX = 0.0;
        private double lastCompositionScaleY = 0.0;

        private bool pendingSizeChange = false;




        private static IntPtr eglDisplay = IntPtr.Zero;

        private IntPtr eglContext;
        private IntPtr eglSurface;
        private IntPtr eglConfig;


        // Config attributes
        public const int EGL_BUFFER_SIZE = 0x3020;
        public const int EGL_ALPHA_SIZE = 0x3021;
        public const int EGL_BLUE_SIZE = 0x3022;
        public const int EGL_GREEN_SIZE = 0x3023;
        public const int EGL_RED_SIZE = 0x3024;
        public const int EGL_DEPTH_SIZE = 0x3025;
        public const int EGL_STENCIL_SIZE = 0x3026;

        // QuerySurface / SurfaceAttrib / CreatePbufferSurface targets
        public const int EGL_HEIGHT = 0x3056;
        public const int EGL_WIDTH = 0x3057;


        // Out-of-band handle values
        public static readonly IntPtr EGL_DEFAULT_DISPLAY = IntPtr.Zero;
        public static readonly IntPtr EGL_NO_CONFIG = IntPtr.Zero;
        public static readonly IntPtr EGL_NO_DISPLAY = IntPtr.Zero;
        public static readonly IntPtr EGL_NO_CONTEXT = IntPtr.Zero;
        public static readonly IntPtr EGL_NO_SURFACE = IntPtr.Zero;

        // Attrib list terminator
        public const int EGL_NONE = 0x3038;

        // CreateContext attributes
        public const int EGL_CONTEXT_CLIENT_VERSION = 0x3098;

        // ANGLE
        public const int EGL_EXPERIMENTAL_PRESENT_PATH_ANGLE = 0x33A4;
        public const int EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE = 0x33AA;
        public const int EGL_EXPERIMENTAL_PRESENT_PATH_COPY_ANGLE = 0x33AA;

        public const int EGL_PLATFORM_ANGLE_TYPE_ANGLE = 0x3203;
        public const int EGL_PLATFORM_ANGLE_MAX_VERSION_MAJOR_ANGLE = 0x3204;
        public const int EGL_PLATFORM_ANGLE_MAX_VERSION_MINOR_ANGLE = 0x3205;
        public const int EGL_PLATFORM_ANGLE_TYPE_DEFAULT_ANGLE = 0x3206;

        public const int EGL_PLATFORM_ANGLE_ANGLE = 0x3202;

        public const int EGL_PLATFORM_ANGLE_TYPE_D3D9_ANGLE = 0x3207;
        public const int EGL_PLATFORM_ANGLE_TYPE_D3D11_ANGLE = 0x3208;
        public const int EGL_PLATFORM_ANGLE_DEVICE_TYPE_ANGLE = 0x3209;
        public const int EGL_PLATFORM_ANGLE_DEVICE_TYPE_HARDWARE_ANGLE = 0x320A;
        public const int EGL_PLATFORM_ANGLE_DEVICE_TYPE_D3D_WARP_ANGLE = 0x320B;
        public const int EGL_PLATFORM_ANGLE_DEVICE_TYPE_D3D_REFERENCE_ANGLE = 0x320C;
        public const int EGL_PLATFORM_ANGLE_ENABLE_AUTOMATIC_TRIM_ANGLE = 0x320F;

        public const int EGL_FIXED_SIZE_ANGLE = 0x3201;

        public const string EGLNativeWindowTypeProperty = "EGLNativeWindowTypeProperty";
        public const string EGLRenderSurfaceSizeProperty = "EGLRenderSurfaceSizeProperty";
        public const string EGLRenderResolutionScaleProperty = "EGLRenderResolutionScaleProperty";




        private void InitializeDisplay()
        {
            if (eglDisplay != IntPtr.Zero)
                return;

            int[] defaultDisplayAttributes = new[]
            {
				// These are the default display attributes, used to request ANGLE's D3D11 renderer.
				// eglInitialize will only succeed with these attributes if the hardware supports D3D11 Feature Level 10_0+.
				Egl.PLATFORM_ANGLE_TYPE_ANGLE, Egl.PLATFORM_ANGLE_TYPE_D3D11_ANGLE,

				// EGL_ANGLE_DISPLAY_ALLOW_RENDER_TO_BACK_BUFFER is an optimization that can have large performance benefits on mobile devices.
				// Its syntax is subject to change, though. Please update your Visual Studio templates if you experience compilation issues with it.
				// Egl.EXPERIMENTAL_PRESENT_PATH_ANGLE, Egl.EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE, 

                

				// EGL_PLATFORM_ANGLE_ENABLE_AUTOMATIC_TRIM_ANGLE is an option that enables ANGLE to automatically call 
				// the IDXGIDevice3::Trim method on behalf of the application when it gets suspended. 
				// Calling IDXGIDevice3::Trim when an application is suspended is a Windows Store application certification requirement.
				Egl.PLATFORM_ANGLE_ENABLE_AUTOMATIC_TRIM_ANGLE, Egl.TRUE,
                Egl.NONE,
            };

            int[] fl9_3DisplayAttributes = new[]
            {
				// These can be used to request ANGLE's D3D11 renderer, with D3D11 Feature Level 9_3.
				// These attributes are used if the call to eglInitialize fails with the default display attributes.
				Egl.PLATFORM_ANGLE_TYPE_ANGLE, Egl.PLATFORM_ANGLE_TYPE_D3D11_ANGLE,
                Egl.PLATFORM_ANGLE_MAX_VERSION_MAJOR_ANGLE, 9,
                Egl.PLATFORM_ANGLE_MAX_VERSION_MINOR_ANGLE, 3,
                EGL_EXPERIMENTAL_PRESENT_PATH_ANGLE, EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE,
                Egl.PLATFORM_ANGLE_ENABLE_AUTOMATIC_TRIM_ANGLE, Egl.TRUE,
                Egl.NONE,
            };

            int[] warpDisplayAttributes = new[]
            {
				// These attributes can be used to request D3D11 WARP.
				// They are used if eglInitialize fails with both the default display attributes and the 9_3 display attributes.
				Egl.PLATFORM_ANGLE_TYPE_ANGLE, Egl.PLATFORM_ANGLE_TYPE_D3D11_ANGLE,
                Egl.PLATFORM_ANGLE_DEVICE_TYPE_ANGLE, EGL_PLATFORM_ANGLE_DEVICE_TYPE_D3D_WARP_ANGLE,
                EGL_EXPERIMENTAL_PRESENT_PATH_ANGLE, EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE,
                Egl.PLATFORM_ANGLE_ENABLE_AUTOMATIC_TRIM_ANGLE, Egl.TRUE,
                Egl.NONE,
            };

            IntPtr config = IntPtr.Zero;

            //
            // To initialize the display, we make three sets of calls to eglGetPlatformDisplayEXT and eglInitialize, with varying 
            // parameters passed to eglGetPlatformDisplayEXT:
            // 1) The first calls uses "defaultDisplayAttributes" as a parameter. This corresponds to D3D11 Feature Level 10_0+.
            // 2) If eglInitialize fails for step 1 (e.g. because 10_0+ isn't supported by the default GPU), then we try again 
            //    using "fl9_3DisplayAttributes". This corresponds to D3D11 Feature Level 9_3.
            // 3) If eglInitialize fails for step 2 (e.g. because 9_3+ isn't supported by the default GPU), then we try again 
            //    using "warpDisplayAttributes".  This corresponds to D3D11 Feature Level 11_0 on WARP, a D3D11 software rasterizer.
            //

            // This tries to initialize EGL to D3D11 Feature Level 10_0+. See above comment for details.
            eglDisplay = Egl.GetPlatformDisplayEXT(Egl.PLATFORM_ANGLE_ANGLE, EGL_DEFAULT_DISPLAY, defaultDisplayAttributes);
            if (eglDisplay == EGL_NO_DISPLAY)
            {
                throw new Exception("Failed to get EGL display");
            }

            if (Egl.Initialize(eglDisplay, out int major, out int minor) == false)
            {
                // This tries to initialize EGL to D3D11 Feature Level 9_3, if 10_0+ is unavailable (e.g. on some mobile devices).
                eglDisplay = Egl.GetPlatformDisplayEXT(EGL_PLATFORM_ANGLE_ANGLE, EGL_DEFAULT_DISPLAY, fl9_3DisplayAttributes);
                if (eglDisplay == EGL_NO_DISPLAY)
                {
                    throw new Exception("Failed to get EGL display");
                }

                if (Egl.Initialize(eglDisplay, out major, out minor) == false)
                {
                    // This initializes EGL to D3D11 Feature Level 11_0 on WARP, if 9_3+ is unavailable on the default GPU.
                    eglDisplay = Egl.GetPlatformDisplayEXT(EGL_PLATFORM_ANGLE_ANGLE, EGL_DEFAULT_DISPLAY, warpDisplayAttributes);
                    if (eglDisplay == EGL_NO_DISPLAY)
                    {
                        throw new Exception("Failed to get EGL display");
                    }

                    if (Egl.Initialize(eglDisplay, out major, out minor) == false)
                    {
                        // If all of the calls to eglInitialize returned EGL_FALSE then an error has occurred.
                        throw new Exception("Failed to initialize EGL");
                    }
                }
            }
        }

        public void InitializeContext()
        {
            int[] configAttributes = new[]
            {
                EGL_RED_SIZE, 8,
                EGL_GREEN_SIZE, 8,
                EGL_BLUE_SIZE, 8,
                EGL_ALPHA_SIZE, 8,
                EGL_DEPTH_SIZE, 8,
                EGL_STENCIL_SIZE, 8,
                EGL_NONE
            };

            int[] contextAttributes = new[]
            {
                EGL_CONTEXT_CLIENT_VERSION, 2,
                EGL_NONE
            };

            IntPtr[] configs = new IntPtr[1];
            if ((Egl.ChooseConfig(eglDisplay, configAttributes, configs, configs.Length, out int numConfigs) == false) || (numConfigs == 0))
            {
                throw new Exception("Failed to choose first EGLConfig");
            }
            eglConfig = configs[0];


            eglContext = Egl.CreateContext(eglDisplay, eglConfig, EGL_NO_CONTEXT, contextAttributes);
            if (eglContext == EGL_NO_CONTEXT)
            {
                throw new Exception("Failed to create EGL context");
            }
        }

        public TextBlock InfoText = new TextBlock
        {
            Text = "Text Block from inside WinUI3 handler",
            VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
            HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Right,
            FontSize = 40.0,
            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 130, 90)),
        };

        public AngleSwapChainPanel()
        {


            //glesContext = null;
            eglConfig = IntPtr.Zero;
            eglContext = IntPtr.Zero;
            eglSurface = IntPtr.Zero;


            lastCompositionScaleX = CompositionScaleX;
            lastCompositionScaleY = CompositionScaleY;


            renderLoopWorker = null;
            renderOnceWorker = null;

            DrawInBackground = false;
            EnableRenderLoop = false;

            ContentsScale = CompositionScaleX;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            CompositionScaleChanged += OnCompositionChanged;
            SizeChanged += OnSizeChanged;

            var binding = new Microsoft.UI.Xaml.Data.Binding
            {
                Path = new PropertyPath(nameof(Visibility)),
                Source = this
            };
            SetBinding(ProxyVisibilityProperty, binding);


            this.Children.Add(InfoText);

            InitializeDisplay();

            InitializeContext();

        }

        public bool DrawInBackground { get; set; }

        public double ContentsScale { get; private set; }

        public bool EnableRenderLoop
        {
            get => enableRenderLoop;
            set
            {
                if (enableRenderLoop != value)
                {
                    enableRenderLoop = value;
                    UpdateRenderLoop(value);
                }
            }
        }

        public void Invalidate()
        {
            if (!isLoaded || EnableRenderLoop)
                return;

            if (DrawInBackground)
            {
                lock (locker)
                {
                    // if we haven't fired a render thread, start one
                    if (renderOnceWorker == null)
                    {
                        renderOnceWorker = ThreadPool.RunAsync(RenderOnce);
                    }
                }
            }
            else
            {
                // draw on this thread, blocking
                RenderFrame();
            }
        }

        LightGraphicsEngine.GradientBackground grb = new LightGraphicsEngine.GradientBackground();


        protected virtual void OnRenderFrame(Rect rect)
        {


            GL.ClearColor(0.5f, 1, 1, 1);
            //GL.Viewport(0, 0, (int)rect.Width, (int)rect.Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            grb.Render();

        }

        protected virtual void OnDestroyingContext()
        {
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {

            isLoaded = true;

            ContentsScale = CompositionScaleX;

            EnsureRenderSurface();
            UpdateRenderLoop(EnableRenderLoop);
            Invalidate();
        }


        public void DestroySurface()
        {
            if (eglDisplay != IntPtr.Zero && eglSurface != IntPtr.Zero)
            {
                Egl.DestroySurface(eglDisplay, eglSurface);
                eglSurface = IntPtr.Zero;
            }
        }

        private void Cleanup()
        {
            if (eglDisplay != IntPtr.Zero && eglContext != IntPtr.Zero)
            {
                Egl.DestroyContext(eglDisplay, eglContext);
                eglContext = IntPtr.Zero;
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            OnDestroyingContext();

            CompositionScaleChanged -= OnCompositionChanged;
            SizeChanged -= OnSizeChanged;

            UpdateRenderLoop(false);
            DestroyRenderSurface();

            isLoaded = false;


            // free unmanaged resources
            DestroySurface();
            Cleanup();

        }

        private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AngleSwapChainPanel panel && e.NewValue is Visibility visibility)
            {
                panel.isVisible = visibility == Visibility.Visible;
                panel.UpdateRenderLoop(panel.isVisible && panel.EnableRenderLoop);
                panel.Invalidate();
            }
        }

        private void OnCompositionChanged(SwapChainPanel sender, object args)
        {
            if (lastCompositionScaleX == CompositionScaleX &&
                lastCompositionScaleY == CompositionScaleY)
            {
                return;
            }

            lastCompositionScaleX = CompositionScaleX;
            lastCompositionScaleY = CompositionScaleY;

            pendingSizeChange = true;

            ContentsScale = CompositionScaleX;

            DestroyRenderSurface();
            EnsureRenderSurface();
            Invalidate();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            pendingSizeChange = true;

            EnsureRenderSurface();
            Invalidate();
        }
        public void CreateSurface(float? resolutionScale)
        {

            // Create an EGL display
            eglDisplay = Egl.GetDisplay(IntPtr.Zero);
            Egl.Initialize(eglDisplay, out _, out _);

            var panel = this;

            IntPtr surface = IntPtr.Zero;

            int[] surfaceAttributes = new int[]
            {
                Egl.NONE // Terminate the list
            };

            // Create a PropertySet and initialize with the EGLNativeWindowType.
            PropertySet surfaceCreationProperties = new PropertySet();
            surfaceCreationProperties.Add(EGLNativeWindowTypeProperty, panel);


            surface = Egl.CreateWindowSurface(
                eglDisplay, 
                eglConfig, 
                surfaceCreationProperties.As<IInspectable>().ThisPtr, 
                Marshal.UnsafeAddrOfPinnedArrayElement(surfaceAttributes, 0)
                );

            if (surface == IntPtr.Zero)
            {
                throw new Exception("Failed to create EGL surface");
            }

            eglSurface = surface;
        }


        private void EnsureRenderSurface()
        {
            
            if (isLoaded && eglSurface == IntPtr.Zero && ActualWidth > 0 && ActualHeight > 0)
            {
                // detach and re-attach the size events as we need to go after the event added by ANGLE
                // otherwise our size will still be the old size

                SizeChanged -= OnSizeChanged;
                CompositionScaleChanged -= OnCompositionChanged;

                CreateSurface(CompositionScaleX);

                SizeChanged += OnSizeChanged;
                CompositionScaleChanged += OnCompositionChanged;
            }
        }

        private void DestroyRenderSurface()
        {
            DestroySurface();
        }

        public void MakeCurrent()
        {
            if (!Egl.MakeCurrent(eglDisplay, eglSurface, eglSurface, eglContext))
            {
                throw new Exception("Failed to make EGLSurface current");
            }
        }

        public bool SwapBuffers()
        {
            return Egl.SwapBuffers(eglDisplay, eglSurface);
        }

        public void GetSurfaceDimensions(out int width, out int height)
        {
            Egl.QuerySurface(eglDisplay, eglSurface, Egl.WIDTH, out width);
            Egl.QuerySurface(eglDisplay, eglSurface, Egl.HEIGHT, out height);
        }

        public void SetViewportSize(int width, int height)
        {
            GL.Viewport(0, 0, width, height);
        }

        private void RenderFrame()
        {
            if (designMode || !isLoaded || !isVisible || eglSurface == IntPtr.Zero)
                return;

            MakeCurrent();

            if (pendingSizeChange)
            {
                pendingSizeChange = false;

                if (!EnableRenderLoop)
                    SwapBuffers();
            }

            GetSurfaceDimensions(out var panelWidth, out var panelHeight);
            SetViewportSize(panelWidth, panelHeight);

            OnRenderFrame(new Rect(0, 0, panelWidth, panelHeight));

            if (!SwapBuffers())
            {
                // The call to eglSwapBuffers might not be successful (i.e. due to Device Lost)
                // If the call fails, then we must reinitialize EGL and the GL resources.
            }
        }

        private void UpdateRenderLoop(bool start)
        {
            if (!isLoaded)
                return;

            lock (locker)
            {
                if (start)
                {
                    // if the render loop is not running, start it
                    if (renderLoopWorker?.Status != AsyncStatus.Started)
                    {
                        renderLoopWorker = ThreadPool.RunAsync(RenderLoop);
                    }
                }
                else
                {
                    // stop the current render loop
                    renderLoopWorker?.Cancel();
                    renderLoopWorker = null;
                }
            }
        }

        private void RenderOnce(IAsyncAction action)
        {
            if (DrawInBackground)
            {
                // run on this background thread
                RenderFrame();
            }
            else
            {
                // run in the main thread, block this one
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, RenderFrame).AsTask().Wait();
            }

            lock (locker)
            {
                // we are finished, so null out
                renderOnceWorker = null;
            }
        }

        private void RenderLoop(IAsyncAction action)
        {
            while (action.Status == AsyncStatus.Started)
            {
                if (DrawInBackground)
                {
                    // run on this background thread
                    RenderFrame();
                }
                else
                {
                    // run in the main thread, block this one
                    var tcs = new TaskCompletionSource();
                    DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
                    {
                        RenderFrame();
                        tcs.SetResult();
                    });
                    tcs.Task.Wait();
                }
            }
        }
    }
}