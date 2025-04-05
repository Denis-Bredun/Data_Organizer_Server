using System.Threading.Tasks;
using System.Collections.Generic;
using Data_Organizer_Server.Models;

namespace Data_Organizer_Server.Interfaces
{
    public interface IPayPalService
    {
        Task<string> GetAccessTokenAsync();
        Task<string> CreatePlanAsync(string productName, string description, decimal price);
        Task<object> GetPlanDetailsAsync(string planId);
        Task<bool> ActivatePlanAsync(string planId);
        Task<bool> DeactivatePlanAsync(string planId);
        Task<string> CreateSubscriptionAsync(string planId);
        Task<object> GetSubscriptionDetailsAsync(string subscriptionId);
        Task<bool> SuspendSubscriptionAsync(string subscriptionId, string reason);
        Task<bool> CancelSubscriptionAsync(string subscriptionId, string reason);
        Task<bool> ActivateSubscriptionAsync(string subscriptionId, string reason);
        Task<bool> CapturePaymentAsync(string subscriptionId);
        Task<List<object>> ListTransactionsAsync(string subscriptionId, string startDate, string endDate);
        Task<string> GenerateClientTokenAsync(string planId);
    }
} 