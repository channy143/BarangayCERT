using MauiApp1.Models;
using System.Windows.Input;

namespace MauiApp1.Pages.Controls
{
    public partial class CertificateRequestView : ContentView
    {
        public CertificateRequestView()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty RequestProperty =
            BindableProperty.Create(nameof(Request), typeof(CertificateRequest), typeof(CertificateRequestView), null);

        public CertificateRequest Request
        {
            get => (CertificateRequest)GetValue(RequestProperty);
            set => SetValue(RequestProperty, value);
        }

        public static readonly BindableProperty ApproveCommandProperty =
            BindableProperty.Create(nameof(ApproveCommand), typeof(ICommand), typeof(CertificateRequestView), null);

        public ICommand ApproveCommand
        {
            get => (ICommand)GetValue(ApproveCommandProperty);
            set => SetValue(ApproveCommandProperty, value);
        }

        public static readonly BindableProperty ApproveCommandParameterProperty =
            BindableProperty.Create(nameof(ApproveCommandParameter), typeof(object), typeof(CertificateRequestView), null);

        public object ApproveCommandParameter
        {
            get => GetValue(ApproveCommandParameterProperty);
            set => SetValue(ApproveCommandParameterProperty, value);
        }

        public static readonly BindableProperty ViewDetailsCommandProperty =
            BindableProperty.Create(nameof(ViewDetailsCommand), typeof(ICommand), typeof(CertificateRequestView), null);

        public ICommand ViewDetailsCommand
        {
            get => (ICommand)GetValue(ViewDetailsCommandProperty);
            set => SetValue(ViewDetailsCommandProperty, value);
        }

        public static readonly BindableProperty ViewDetailsCommandParameterProperty =
            BindableProperty.Create(nameof(ViewDetailsCommandParameter), typeof(object), typeof(CertificateRequestView), null);

        public object ViewDetailsCommandParameter
        {
            get => GetValue(ViewDetailsCommandParameterProperty);
            set => SetValue(ViewDetailsCommandParameterProperty, value);
        }

        public static readonly BindableProperty DeleteCommandProperty =
            BindableProperty.Create(nameof(DeleteCommand), typeof(ICommand), typeof(CertificateRequestView), null);

        public ICommand DeleteCommand
        {
            get => (ICommand)GetValue(DeleteCommandProperty);
            set => SetValue(DeleteCommandProperty, value);
        }

        public static readonly BindableProperty DeleteCommandParameterProperty =
            BindableProperty.Create(nameof(DeleteCommandParameter), typeof(object), typeof(CertificateRequestView), null);

        public object DeleteCommandParameter
        {
            get => GetValue(DeleteCommandParameterProperty);
            set => SetValue(DeleteCommandParameterProperty, value);
        }
    }
}
