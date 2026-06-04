using Microsoft.EntityFrameworkCore;
using OnlineInsuranceManagementSystem.Data;
using OnlineInsuranceManagementSystem.Models;
using System.Threading.Tasks;

namespace OnlineInsuranceManagementSystem.Services
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(int userId, string title, string message,
            string type, string module, int? recordId = null,
            string priority = "Normal", string actionUrl = null);

        Task<int> GetUnreadCountAsync(int userId);
    }

    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateNotificationAsync(int userId, string title, string message,
            string type, string module, int? recordId = null,
            string priority = "Normal", string actionUrl = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                NotificationType = type,
                RelatedModule = module,
                RelatedRecordId = recordId,
                Priority = priority,
                ActionUrl = actionUrl,
                CreatedDate = DateTime.Now,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }
    }
}