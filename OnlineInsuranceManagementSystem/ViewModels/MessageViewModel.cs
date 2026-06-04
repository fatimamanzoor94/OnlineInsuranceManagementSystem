using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineInsuranceManagementSystem.ViewModels
{
    public class MessageViewModel
    {
        public int MessageId { get; set; }

        public int SenderId { get; set; }
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        public string MessageText { get; set; } = string.Empty;

        public DateTime SentDate { get; set; }

        public bool IsRead { get; set; }

        // === Display Helpers ===
        public string MessageIdDisplay => $"#MSG-{MessageId:D4}";

        public string SentDateDisplay => SentDate.ToString("dd-MMM-yyyy");

        public string MessagePreview =>
            string.IsNullOrEmpty(MessageText) ? "-" :
            (MessageText.Length > 50 ? MessageText.Substring(0, 50) + "..." : MessageText);

        public string StatusBadgeClass => IsRead
            ? "bg-secondary-subtle text-secondary"
            : "bg-primary-subtle text-primary";

        public string StatusText => IsRead ? "Read" : "Unread";
    }
}

