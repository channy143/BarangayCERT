using CommunityToolkit.Mvvm.ComponentModel;
using MauiApp1.Models;

namespace MauiApp1.Services
{
    public partial class AuthStateService : ObservableObject
    {
        private readonly SupabaseAuthService _authService;

        [ObservableProperty]
        private User? _currentUser;

        [ObservableProperty]
        private bool _isAuthenticated;

        [ObservableProperty]
        private bool _isAdmin;

        public AuthStateService(SupabaseAuthService authService)
        {
            _authService = authService;
        }

        public async Task InitializeAsync()
        {
            await _authService.InitializeAsync();
            await RefreshAuthStateAsync();
        }

        public async Task RefreshAuthStateAsync()
        {
            CurrentUser = await _authService.GetCurrentUserAsync();
            IsAuthenticated = CurrentUser != null;
            IsAdmin = CurrentUser?.IsAdmin ?? false;
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            var user = await _authService.SignInAsync(email, password);
            if (user != null)
            {
                CurrentUser = user;
                IsAuthenticated = true;
                IsAdmin = user.IsAdmin;
                return true;
            }
            return false;
        }

        public async Task<bool> SignUpAsync(string email, string password, UserRole role = UserRole.User)
        {
            var user = await _authService.SignUpAsync(email, password, role);
            if (user != null)
            {
                CurrentUser = user;
                IsAuthenticated = true;
                IsAdmin = user.IsAdmin;
                return true;
            }
            return false;
        }

        public async Task LogoutAsync()
        {
            await _authService.SignOutAsync();
            CurrentUser = null;
            IsAuthenticated = false;
            IsAdmin = false;
        }
    }
}
