using Microsoft.Extensions.DependencyInjection;

namespace MauiApp1
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var shell = new AppShell();

            // Initialize auth state after shell is created
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await shell.InitializeAuthAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Auth initialization error: {ex.Message}");
                }
            });

            return new Window(shell);
        }
    }
}