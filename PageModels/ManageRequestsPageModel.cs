using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1.Data;
using MauiApp1.Models;
using MauiApp1.Services;
using System.Collections.ObjectModel;

namespace MauiApp1.PageModels
{
    public partial class ManageRequestsPageModel : ObservableObject
    {
        private readonly CertificateRequestRepository _certificateRequestRepository;
        private readonly AuthStateService _authStateService;
        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        private ObservableCollection<CertificateRequest> _pendingRequests = new();

        [ObservableProperty]
        private ObservableCollection<CertificateRequest> _allRequests = new();

        [ObservableProperty]
        private bool _isBusy = false;

        [ObservableProperty]
        private bool _isRefreshing = false;

        [ObservableProperty]
        private int _pendingCount = 0;

        [ObservableProperty]
        private CertificateRequest? _selectedRequest;

        [ObservableProperty]
        private string _adminNotes = string.Empty;

        [ObservableProperty]
        private bool _showApprovalDialog = false;

        public ManageRequestsPageModel(
            CertificateRequestRepository certificateRequestRepository,
            AuthStateService authStateService,
            ModalErrorHandler errorHandler)
        {
            _certificateRequestRepository = certificateRequestRepository;
            _authStateService = authStateService;
            _errorHandler = errorHandler;
        }

        [RelayCommand]
        private async Task LoadRequestsAsync()
        {
            if (!_authStateService.IsAdmin)
            {
                await AppShell.DisplaySnackbarAsync("You do not have permission to access this page.");
                await Shell.Current.GoToAsync("//main");
                return;
            }

            try
            {
                IsBusy = true;

                var allRequests = await _certificateRequestRepository.ListAsync();
                var pendingRequests = allRequests.Where(r => r.IsPending).ToList();

                PendingRequests = new ObservableCollection<CertificateRequest>(pendingRequests);
                AllRequests = new ObservableCollection<CertificateRequest>(allRequests);
                PendingCount = pendingRequests.Count;
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            try
            {
                IsRefreshing = true;
                await LoadRequestsAsync();
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private void OpenApprovalDialog(CertificateRequest request)
        {
            SelectedRequest = request;
            AdminNotes = string.Empty;
            ShowApprovalDialog = true;
        }

        [RelayCommand]
        private async Task ApproveRequestAsync()
        {
            if (SelectedRequest == null) return;

            try
            {
                IsBusy = true;

                await _certificateRequestRepository.UpdateStatusAsync(
                    SelectedRequest.Id,
                    CertificateStatus.Approved,
                    AdminNotes);

                ShowApprovalDialog = false;
                await AppShell.DisplayToastAsync("Request approved successfully!");
                await LoadRequestsAsync();
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task RejectRequestAsync()
        {
            if (SelectedRequest == null) return;

            bool confirm = await Shell.Current.DisplayAlert(
                "Reject Request",
                $"Are you sure you want to reject the request for {SelectedRequest.CertificateType}?",
                "Reject",
                "Cancel");

            if (!confirm) return;

            try
            {
                IsBusy = true;

                await _certificateRequestRepository.UpdateStatusAsync(
                    SelectedRequest.Id,
                    CertificateStatus.Rejected,
                    AdminNotes);

                ShowApprovalDialog = false;
                await AppShell.DisplayToastAsync("Request rejected.");
                await LoadRequestsAsync();
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void CloseApprovalDialog()
        {
            ShowApprovalDialog = false;
            SelectedRequest = null;
            AdminNotes = string.Empty;
        }

        [RelayCommand]
        private async Task ViewRequestDetailsAsync(CertificateRequest request)
        {
            var details = $"Type: {request.CertificateType}\n" +
                         $"Purpose: {request.Purpose}\n" +
                         $"Requested by: {request.Requester?.Email ?? "Unknown"}\n" +
                         $"Request Date: {request.RequestDateDisplay}\n" +
                         $"Status: {request.StatusDisplay}\n";

            if (request.ProcessedDate.HasValue)
            {
                details += $"Processed: {request.ProcessedDateDisplay}\n";
            }

            if (!string.IsNullOrEmpty(request.AdminNotes))
            {
                details += $"\nAdmin Notes: {request.AdminNotes}";
            }

            await Shell.Current.DisplayAlert("Request Details", details, "OK");
        }

        [RelayCommand]
        private async Task DeleteRequestAsync(CertificateRequest request)
        {
            bool confirm = await Shell.Current.DisplayAlert(
                "Delete Request",
                $"Are you sure you want to delete this request for {request.CertificateType}?",
                "Delete",
                "Cancel");

            if (!confirm) return;

            try
            {
                IsBusy = true;
                await _certificateRequestRepository.DeleteAsync(request.Id);
                await AppShell.DisplayToastAsync("Request deleted.");
                await LoadRequestsAsync();
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task AppearingAsync()
        {
            await LoadRequestsAsync();
        }
    }
}
