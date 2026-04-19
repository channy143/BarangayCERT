using MauiApp1.Models;
using System.Globalization;

namespace MauiApp1.Utilities
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CertificateStatus status)
            {
                return status switch
                {
                    CertificateStatus.Pending => Color.FromArgb("#FFC107"),  // Yellow
                    CertificateStatus.Approved => Color.FromArgb("#28A745"), // Green
                    CertificateStatus.Rejected => Color.FromArgb("#DC3545"), // Red
                    _ => Color.FromArgb("#6C757D") // Gray
                };
            }
            return Color.FromArgb("#6C757D");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
