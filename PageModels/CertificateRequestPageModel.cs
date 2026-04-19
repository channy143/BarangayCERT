using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1.Data;
using MauiApp1.Models;
using MauiApp1.Services;

namespace MauiApp1.PageModels
{
    public partial class CertificateRequestPageModel : ObservableObject
    {
        private readonly CertificateRequestRepository _certificateRequestRepository;
        private readonly AuthStateService _authStateService;
        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        private string _certificateType = string.Empty;

        [ObservableProperty]
        private string _purpose = string.Empty;

        [ObservableProperty]
        private bool _isBusy = false;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public List<string> CertificateTypes { get; } = new()
        {
            "Birth Certificate",
            "Marriage Certificate",
            "Death Certificate",
            "Business Permit",
            "Tax Clearance",
            "Barangay Clearance",
            "Police Clearance",
            "Other"
        };

        public CertificateRequestPageModel(
            CertificateRequestRepository certificateRequestRepository,
            AuthStateService authStateService,
            ModalErrorHandler errorHandler)
        {
            _certificateRequestRepository = certificateRequestRepository;
            _authStateService = authStateService;
            _errorHandler = errorHandler;
        }

        [RelayCommand]
        private async Task SubmitRequestAsync()
        {
            if (string.IsNullOrWhiteSpace(CertificateType))
            {
                ErrorMessage = "Please select a certificate type.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Purpose))
            {
                ErrorMessage = "Please enter the purpose of this request.";
                return;
            }

            var currentUser = _authStateService.CurrentUser;
            if (currentUser == null)
            {
                ErrorMessage = "You must be logged in to request a certificate.";
                return;
            }

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                var request = new CertificateRequest
                {
                    UserId = currentUser.Id,
                    CertificateType = CertificateType,
                    Purpose = Purpose,
                    Status = CertificateStatus.Pending,
                    RequestDate = DateTime.UtcNow
                };

                await _certificateRequestRepository.SaveAsync(request);

                // Clear form
                CertificateType = string.Empty;
                Purpose = string.Empty;

                await AppShell.DisplayToastAsync("Certificate request submitted successfully!");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error submitting request: {ex.Message}";
                _errorHandler.HandleError(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
