namespace MauiApp1.Pages
{
    public partial class ManageRequestsPage : ContentPage
    {
        public ManageRequestsPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is PageModels.ManageRequestsPageModel vm)
            {
                await vm.AppearingAsync();
            }
        }
    }
}
