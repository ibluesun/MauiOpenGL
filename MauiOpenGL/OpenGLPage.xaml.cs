namespace MauiOpenGL;

public partial class OpenGLPage : ContentPage
{
	public OpenGLPage()
	{
		InitializeComponent();

		
	}

    private void RefreshButton_Clicked(object sender, EventArgs e)
    {
		MauiOpenGLViral.Invalidate();
    }
}