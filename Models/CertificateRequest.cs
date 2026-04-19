namespace MauiApp1.Models
{
    public enum CertificateStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class CertificateRequest
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string CertificateType { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public CertificateStatus Status { get; set; } = CertificateStatus.Pending;
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedDate { get; set; }
        public string? AdminNotes { get; set; }

        // Navigation property
        public User? Requester { get; set; }

        public string StatusDisplay => Status.ToString();
        public bool IsPending => Status == CertificateStatus.Pending;
        public bool IsApproved => Status == CertificateStatus.Approved;
        public bool IsRejected => Status == CertificateStatus.Rejected;

        public string RequestDateDisplay => RequestDate.ToLocalTime().ToString("MMM dd, yyyy HH:mm");
        public string? ProcessedDateDisplay => ProcessedDate?.ToLocalTime().ToString("MMM dd, yyyy HH:mm");
    }
}
