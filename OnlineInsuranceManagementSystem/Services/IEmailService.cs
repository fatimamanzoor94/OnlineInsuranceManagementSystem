using System.Threading.Tasks;

namespace OnlineInsuranceManagementSystem.Services
{
    public interface IEmailService
    {
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string fullName, string resetLink);

        // ✅ New Agent Email Methods
        Task<bool> SendAgentAccountCreatedEmailAsync(string toEmail, string fullName, string password);
        Task<bool> SendAgentApprovalEmailAsync(string toEmail, string fullName);

        // Send Reply Email to Customer
        Task<bool> SendReplyToCustomerEmailAsync(string toEmail, string customerName, string subject, string message, string originalMessageId);

        Task SendAgentAssignmentNotificationAsync(string agentEmail, int applicationId, string subject);
    }
}
