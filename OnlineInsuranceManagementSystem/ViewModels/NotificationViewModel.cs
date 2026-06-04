using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineInsuranceManagementSystem.ViewModels
{
    public class NotificationViewModel
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }

        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Message")]
        public string Message { get; set; } = string.Empty;

        [Display(Name = "Type")]
        public string NotificationType { get; set; } = "General";

        [Display(Name = "Status")]
        public string StatusText => IsRead ? "Read" : "Unread";

        [Display(Name = "Status Badge")]
        public string StatusBadgeClass => IsRead
            ? "bg-success-subtle text-success"
            : "bg-warning-subtle text-warning";

        public bool IsRead { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Date Display")]
        public string CreatedDateDisplay => CreatedDate.ToString("dd MMM yyyy, hh:mm tt");

        public int? RelatedRecordId { get; set; }
        public string? RelatedModule { get; set; }

        [Display(Name = "Module Badge")]
        public string ModuleBadgeClass => RelatedModule?.ToLower() switch
        {
            "applications" => "bg-label-primary",
            "claims" => "bg-label-success",
            "payments" => "bg-label-info",
            "messages" => "bg-label-secondary",
            "users" => "bg-label-warning",
            "loans" => "bg-label-danger",
            _ => "bg-label-secondary"
        };

        public string Priority { get; set; } = "Normal";

        [Display(Name = "Priority Badge")]
        public string PriorityBadgeClass => Priority?.ToLower() switch
        {
            "critical" => "bg-danger text-white",
            "high" => "bg-warning text-dark",
            "normal" => "bg-info text-white",
            "low" => "bg-secondary text-white",
            _ => "bg-secondary text-white"
        };

        public string? ActionUrl { get; set; }

        // Display helpers
        public string NotificationIdDisplay => $"#NOT-{NotificationId:D4}";
        public string MessagePreview => Message.Length > 80 ? Message.Substring(0, 80) + "..." : Message;
    }
}