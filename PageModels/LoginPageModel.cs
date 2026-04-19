using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1.Services;

namespace MauiApp1.PageModels
{
    public partial class LoginPageModel : ObservableObject
    {
        private readonly AuthStateService _authStateService;
        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _confirmPassword = string.Empty;

        [ObservableProperty]
        private bool _isLoginMode = true;

        [ObservableProperty]
        private bool _isBusy = false;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public string Title => IsLoginMode ? "Sign In" : "Create Account";
        public string ButtonText => IsLoginMode ? "Sign In" : "Sign Up";
        public string TogglePrompt => IsLoginMode ? "Don't have an account?" : "Already have an account?";
        public string ToggleAction => IsLoginMode ? "Sign Up" : "Sign In";

        partial void OnIsLoginModeChanged(bool value)
        {
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(ButtonText));
            OnPropertyChanged(nameof(TogglePrompt));
            OnPropertyChanged(nameof(ToggleAction));
        }

        public LoginPageModel(AuthStateService authStateService, ModalErrorHandler errorHandler)
        {
            _authStateService = authStateService;
            _errorHandler = errorHandler;
        }

        [RelayCommand]
        private void ToggleMode()
        {
            IsLoginMode = !IsLoginMode;
            ErrorMessage = string.Empty;
        }

        [RelayCommand]
        private async Task SubmitAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Email and password are required.";
                return;
            }

            if (!IsLoginMode && Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match.";
                return;
            }

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                bool success;
                if (IsLoginMode)
                {
                    success = await _authStateService.LoginAsync(Email, Password);
                }
                else
                {
                    success = await _authStateService.SignUpAsync(Email, Password);
                }

                if (success)
                {
                    // Navigate to main page
                    await Shell.Current.GoToAsync("//main");
                }
                else
                {
                    ErrorMessage = IsLoginMode ? "Invalid email or password." : "Failed to create account.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                _errorHandler.HandleError(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
