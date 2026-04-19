using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Font = Microsoft.Maui.Font;
using MauiApp1.Services;

namespace MauiApp1
{
    public partial class AppShell : Shell
    {
        private readonly AuthStateService _authStateService;

        public AppShell()
        {
            InitializeComponent();
            var currentTheme = Application.Current!.RequestedTheme;
            ThemeSegmentedControl.SelectedIndex = currentTheme == AppTheme.Light ? 0 : 1;

            // Get AuthStateService from DI
            _authStateService = Handler?.MauiContext?.Services.GetService<AuthStateService>()
                ?? throw new InvalidOperationException("AuthStateService not found");

            // Subscribe to auth state changes
            _authStateService.PropertyChanged += OnAuthStateChanged;
        }

        private void OnAuthStateChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AuthStateService.IsAuthenticated) ||
                e.PropertyName == nameof(AuthStateService.IsAdmin))
            {
                MainThread.BeginInvokeOnMainThread(UpdateFlyoutVisibility);
            }
        }

        private void UpdateFlyoutVisibility()
        {
            // Hide login page when authenticated
            LoginShellContent.FlyoutItemIsVisible = !_authStateService.IsAuthenticated;

            // Show main pages when authenticated
            MainShellContent.FlyoutItemIsVisible = _authStateService.IsAuthenticated;
            ProjectsShellContent.FlyoutItemIsVisible = _authStateService.IsAuthenticated;
            ManageMetaShellContent.FlyoutItemIsVisible = _authStateService.IsAuthenticated;
            CertificateRequestShellContent.FlyoutItemIsVisible = _authStateService.IsAuthenticated;

            // Show manage requests only for admins
            ManageRequestsShellContent.FlyoutItemIsVisible = _authStateService.IsAdmin;
        }

        public async Task InitializeAuthAsync()
        {
            try
            {
                await _authStateService.InitializeAsync();
                UpdateFlyoutVisibility();

                // Navigate to login if not authenticated, otherwise to main
                if (!_authStateService.IsAuthenticated)
                {
                    await GoToAsync("//login");
                }
                else
                {
                    await GoToAsync("//main");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Auth initialization error: {ex.Message}");
                // Show all content and default to login page
                UpdateFlyoutVisibility();
                await GoToAsync("//login");
            }
        }
        public static async Task DisplaySnackbarAsync(string message)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            var snackbarOptions = new SnackbarOptions
            {
                BackgroundColor = Color.FromArgb("#FF3300"),
                TextColor = Colors.White,
                ActionButtonTextColor = Colors.Yellow,
                CornerRadius = new CornerRadius(0),
                Font = Font.SystemFontOfSize(18),
                ActionButtonFont = Font.SystemFontOfSize(14)
            };

            var snackbar = Snackbar.Make(message, visualOptions: snackbarOptions);

            await snackbar.Show(cancellationTokenSource.Token);
        }

        public static async Task DisplayToastAsync(string message)
        {
            // Toast is currently not working in MCT on Windows
            if (OperatingSystem.IsWindows())
                return;

            var toast = Toast.Make(message, textSize: 18);

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await toast.Show(cts.Token);
        }

        private void SfSegmentedControl_SelectionChanged(object? sender, Syncfusion.Maui.Toolkit.SegmentedControl.SelectionChangedEventArgs e)
        {
            Application.Current!.UserAppTheme = e.NewIndex == 0 ? AppTheme.Light : AppTheme.Dark;
        }

        protected override void OnNavigating(ShellNavigatingEventArgs args)
        {
            base.OnNavigating(args);

            // Prevent navigation to authenticated pages if not logged in
            if (!_authStateService.IsAuthenticated &&
                args.Target.Location.OriginalString != "//login")
            {
                args.Cancel();
                _ = GoToAsync("//login");
            }
        }
    }
}
