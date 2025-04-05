using Data_Organizer_Server.Interfaces;
using Data_Organizer_Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Data_Organizer_Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/paypal")]
    public class PayPalController : ControllerBase
    {
        private readonly IPayPalService _payPalService;
        private readonly ILogger<PayPalController> _logger;

        public PayPalController(IPayPalService payPalService, ILogger<PayPalController> logger)
        {
            _payPalService = payPalService;
            _logger = logger;
        }

        [HttpPost("plans")]
        public async Task<IActionResult> CreatePlan([FromBody] PlanRequest request)
        {
            _logger.LogInformation("Creating PayPal plan");
            
            if (request == null || string.IsNullOrWhiteSpace(request.ProductName) || 
                string.IsNullOrWhiteSpace(request.Description) || request.Price <= 0)
            {
                var error = "Invalid plan request. ProductName, Description and Price (> 0) are required.";
                _logger.LogWarning(error);
                return BadRequest(new { Error = error });
            }

            try
            {
                var planId = await _payPalService.CreatePlanAsync(request.ProductName, request.Description, request.Price);
                _logger.LogInformation("Successfully created PayPal plan with ID: {PlanId}", planId);
                return Ok(new { plan_id = planId });
            }
            catch (HttpRequestException ex)
            {
                string error = "Error connecting to PayPal API. Please try again later.";
                _logger.LogError(ex, error);
                return StatusCode(502, new { Error = error });
            }
            catch (Exception ex)
            {
                string error = "An error occurred while creating the plan.";
                _logger.LogError(ex, "Error in CreatePlan");
                return StatusCode(500, new { Error = error, Details = ex.Message });
            }
        }

        [HttpGet("plans/{planId}")]
        public async Task<IActionResult> GetPlanDetails(string planId)
        {
            _logger.LogInformation("Getting details for PayPal plan {PlanId}", planId);
            
            if (string.IsNullOrWhiteSpace(planId))
            {
                var error = "Plan ID is required.";
                _logger.LogWarning(error);
                return BadRequest(new { Error = error });
            }

            try
            {
                var plan = await _payPalService.GetPlanDetailsAsync(planId);
                _logger.LogInformation("Successfully retrieved plan details for plan ID: {PlanId}", planId);
                return Ok(plan);
            }
            catch (HttpRequestException ex)
            {
                string error = "Error connecting to PayPal API. Please try again later.";
                _logger.LogError(ex, error);
                return StatusCode(502, new { Error = error });
            }
            catch (Exception ex)
            {
                string error = "An error occurred while retrieving the plan details.";
                _logger.LogError(ex, "Error in GetPlanDetails for plan {PlanId}", planId);
                return StatusCode(500, new { Error = error, Details = ex.Message });
            }
        }

        [HttpPost("plans/{planId}/activate")]
        public async Task<IActionResult> ActivatePlan(string planId)
        {
            _logger.LogInformation("Activating PayPal plan {PlanId}", planId);
            
            if (string.IsNullOrWhiteSpace(planId))
            {
                var error = "Plan ID is required.";
                _logger.LogWarning(error);
                return BadRequest(new { Error = error });
            }

            try
            {
                var result = await _payPalService.ActivatePlanAsync(planId);
                _logger.LogInformation("Successfully activated plan ID: {PlanId}", planId);
                return Ok(new { success = result });
            }
            catch (HttpRequestException ex)
            {
                string error = "Error connecting to PayPal API. Please try again later.";
                _logger.LogError(ex, error);
                return StatusCode(502, new { Error = error });
            }
            catch (Exception ex)
            {
                string error = "An error occurred while activating the plan.";
                _logger.LogError(ex, "Error in ActivatePlan for plan {PlanId}", planId);
                return StatusCode(500, new { Error = error, Details = ex.Message });
            }
        }

        [HttpPost("plans/{planId}/deactivate")]
        public async Task<IActionResult> DeactivatePlan(string planId)
        {
            _logger.LogInformation("Deactivating PayPal plan {PlanId}", planId);
            
            if (string.IsNullOrWhiteSpace(planId))
            {
                var error = "Plan ID is required.";
                _logger.LogWarning(error);
                return BadRequest(new { Error = error });
            }

            try
            {
                var result = await _payPalService.DeactivatePlanAsync(planId);
                _logger.LogInformation("Successfully deactivated plan ID: {PlanId}", planId);
                return Ok(new { success = result });
            }
            catch (HttpRequestException ex)
            {
                string error = "Error connecting to PayPal API. Please try again later.";
                _logger.LogError(ex, error);
                return StatusCode(502, new { Error = error });
            }
            catch (Exception ex)
            {
                string error = "An error occurred while deactivating the plan.";
                _logger.LogError(ex, "Error in DeactivatePlan for plan {PlanId}", planId);
                return StatusCode(500, new { Error = error, Details = ex.Message });
            }
        }

        [HttpPost("subscriptions")]
        public async Task<IActionResult> CreateSubscription([FromBody] SubscriptionRequest request)
        {
            _logger.LogInformation("Creating PayPal subscription for plan {PlanId}", request?.PlanId);
            
            if (request == null || string.IsNullOrWhiteSpace(request.PlanId))
            {
                var error = "Plan ID is required.";
                _logger.LogWarning(error);
                return BadRequest(new { Error = error });
            }

            try
            {
                var subscriptionInfo = await _payPalService.CreateSubscriptionAsync(request.PlanId);
                _logger.LogInformation("Successfully created subscription for plan ID: {PlanId}", request.PlanId);
                return Ok(new { subscription_info = subscriptionInfo });
            }
            catch (HttpRequestException ex)
            {
                string error = "Error connecting to PayPal API. Please try again later.";
                _logger.LogError(ex, error);
                return StatusCode(502, new { Error = error });
            }
            catch (Exception ex)
            {
                string error = "An error occurred while creating the subscription.";
                _logger.LogError(ex, "Error in CreateSubscription for plan {PlanId}", request.PlanId);
                return StatusCode(500, new { Error = error, Details = ex.Message });
            }
        }

        [HttpGet("subscriptions/{subscriptionId}")]
        public async Task<IActionResult> GetSubscriptionDetails(string subscriptionId)
        {
            _logger.LogInformation("Getting details for PayPal subscription {SubscriptionId}", subscriptionId);
            
            if (string.IsNullOrWhiteSpace(subscriptionId))
            {
                var error = "Subscription ID is required.";
                _logger.LogWarning(error);
                return BadRequest(new { Error = error });
            }

            try
            {
                var subscription = await _payPalService.GetSubscriptionDetailsAsync(subscriptionId);
                _logger.LogInformation("Successfully retrieved subscription details for ID: {SubscriptionId}", subscriptionId);
                return Ok(subscription);
            }
            catch (HttpRequestException ex)
            {
                string error = "Error connecting to PayPal API. Please try again later.";
                _logger.LogError(ex, error);
                return StatusCode(502, new { Error = error });
            }
            catch (Exception ex)
            {
                string error = "An error occurred while retrieving the subscription details.";
                _logger.LogError(ex, "Error in GetSubscriptionDetails for subscription {SubscriptionId}", subscriptionId);
                return StatusCode(500, new { Error = error, Details = ex.Message });
            }
        }

        [HttpPost("subscriptions/{subscriptionId}/suspend")]
        public async Task<IActionResult> SuspendSubscription(string subscriptionId, [FromBody] ReasonRequest request)
        {
            _logger.LogInformation("Suspending PayPal subscription {SubscriptionId}", subscriptionId);
            
            if (string.IsNullOrWhiteSpace(subscriptionId))
            {
                var error = "Subscription ID is required.";
                _logger.LogWarning(error);
                return BadRequest(new { Error = error });
            }

            if (request == null || string.IsNullOrWhiteSpace(request.Reason))
            {
                request = new ReasonRequest { Reason = "User requested suspension" };
            }

            try
            {
                var result = await _payPalService.SuspendSubscriptionAsync(subscriptionId, request.Reason);
                _logger.LogInformation("Successfully suspended subscription ID: {SubscriptionId}", subscriptionId);
                return Ok(new { success = result });
            }
            catch (HttpRequestException ex)
            {
                string error = "Error connecting to PayPal API. Please try again later.";
                _logger.LogError(ex, error);
                return StatusCode(502, new { Error = error });
            }
            catch (Exception ex)
            {
                string error = "An error occurred while suspending the subscription.";
                _logger.LogError(ex, "Error in SuspendSubscription for subscription {SubscriptionId}", subscriptionId);
                return StatusCode(500, new { Error = error, Details = ex.Message });
            }
        }

        [HttpPost("subscriptions/{subscriptionId}/cancel")]
        public async Task<IActionResult> CancelSubscription(string subscriptionId, [FromBody] ReasonRequest request)
        {
            _logger.LogInformation("Cancelling PayPal subscription {SubscriptionId}", subscriptionId);
            
            if (string.IsNullOrWhiteSpace(subscriptionId))
            {
                var error = "Subscription ID is required.";
                _logger.LogWarning(error);
                return BadRequest(new { Error = error });
            }

            if (request == null || string.IsNullOrWhiteSpace(request.Reason))
            {
                request = new ReasonRequest { Reason = "User requested cancellation" };
            }

            try
            {
                var result = await _payPalService.CancelSubscriptionAsync(subscriptionId, request.Reason);
                _logger.LogInformation("Successfully cancelled subscription ID: {SubscriptionId}", subscriptionId);
                return Ok(new { success = result });
            }
            catch (HttpRequestException ex)
            {
                string error = "Error connecting to PayPal API. Please try again later.";
                _logger.LogError(ex, error);
                return StatusCode(502, new { Error = error });
            }
            catch (Exception ex)
            {
                string error = "An error occurred while cancelling the subscription.";
                _logger.LogError(ex, "Error in CancelSubscription for subscription {SubscriptionId}", subscriptionId);
                return StatusCode(500, new { Error = error, Details = ex.Message });
            }
        }

        [HttpPost("subscriptions/{subscriptionId}/activate")]
        public async Task<IActionResult> ActivateSubscription(string subscriptionId, [FromBody] ReasonRequest request)
        {
            _logger.LogInformation("Activating PayPal subscription {SubscriptionId}", subscriptionId);
            
            if (string.IsNullOrWhiteSpace(subscriptionId))
            {
                var error = "Subscription ID is required.";
                _logger.LogWarning(error);
                return BadRequest(new { Error = error });
            }

            if (request == null || string.IsNullOrWhiteSpace(request.Reason))
            {
                request = new ReasonRequest { Reason = "User requested activation" };
            }

            try
            {
                var result = await _payPalService.ActivateSubscriptionAsync(subscriptionId, request.Reason);
                _logger.LogInformation("Successfully activated subscription ID: {SubscriptionId}", subscriptionId);
                return Ok(new { success = result });
            }
            catch (HttpRequestException ex)
            {
                string error = "Error connecting to PayPal API. Please try again later.";
                _logger.LogError(ex, error);
                return StatusCode(502, new { Error = error });
            }
            catch (Exception ex)
            {
                string error = "An error occurred while activating the subscription.";
                _logger.LogError(ex, "Error in ActivateSubscription for subscription {SubscriptionId}", subscriptionId);
                return StatusCode(500, new { Error = error, Details = ex.Message });
            }
        }

        [HttpPost("subscriptions/{subscriptionId}/capture")]
        public async Task<IActionResult> CapturePayment(string subscriptionId)
        {
            _logger.LogInformation("Capturing payment for PayPal subscription {SubscriptionId}", subscriptionId);
            
            if (string.IsNullOrWhiteSpace(subscriptionId))
            {
                var error = "Subscription ID is required.";
                _logger.LogWarning(error);
                return BadRequest(new { Error = error });
            }

            try
            {
                var result = await _payPalService.CapturePaymentAsync(subscriptionId);
                _logger.LogInformation("Successfully captured payment for subscription ID: {SubscriptionId}", subscriptionId);
                return Ok(new { success = result });
            }
            catch (HttpRequestException ex)
            {
                string error = "Error connecting to PayPal API. Please try again later.";
                _logger.LogError(ex, error);
                return StatusCode(502, new { Error = error });
            }
            catch (Exception ex)
            {
                string error = "An error occurred while capturing the payment.";
                _logger.LogError(ex, "Error in CapturePayment for subscription {SubscriptionId}", subscriptionId);
                return StatusCode(500, new { Error = error, Details = ex.Message });
            }
        }

        [HttpGet("subscriptions/{subscriptionId}/transactions")]
        public async Task<IActionResult> ListTransactions(string subscriptionId, [FromQuery] string startDate, [FromQuery] string endDate)
        {
            _logger.LogInformation("Listing transactions for PayPal subscription {SubscriptionId}", subscriptionId);
            
            if (string.IsNullOrWhiteSpace(subscriptionId))
            {
                var error = "Subscription ID is required.";
                _logger.LogWarning(error);
                return BadRequest(new { Error = error });
            }

            if (string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(endDate))
            {
                var error = "Start date and end date are required.";
                _logger.LogWarning(error);
                return BadRequest(new { Error = error });
            }

            try
            {
                var transactions = await _payPalService.ListTransactionsAsync(subscriptionId, startDate, endDate);
                _logger.LogInformation("Successfully listed transactions for subscription ID: {SubscriptionId}", subscriptionId);
                return Ok(new { transactions = transactions });
            }
            catch (HttpRequestException ex)
            {
                string error = "Error connecting to PayPal API. Please try again later.";
                _logger.LogError(ex, error);
                return StatusCode(502, new { Error = error });
            }
            catch (Exception ex)
            {
                string error = "An error occurred while listing the transactions.";
                _logger.LogError(ex, "Error in ListTransactions for subscription {SubscriptionId}", subscriptionId);
                return StatusCode(500, new { Error = error, Details = ex.Message });
            }
        }

        [HttpGet("client-token/{planId}")]
        public async Task<IActionResult> GetClientToken(string planId)
        {
            _logger.LogInformation("Generating client token for plan {PlanId}", planId);
            
            if (string.IsNullOrWhiteSpace(planId))
            {
                var error = "Plan ID is required.";
                _logger.LogWarning(error);
                return BadRequest(new { Error = error });
            }

            try
            {
                var clientToken = await _payPalService.GenerateClientTokenAsync(planId);
                _logger.LogInformation("Successfully generated client token for plan ID: {PlanId}", planId);
                return Ok(new { client_token = clientToken });
            }
            catch (HttpRequestException ex)
            {
                string error = "Error connecting to PayPal API. Please try again later.";
                _logger.LogError(ex, error);
                return StatusCode(502, new { Error = error });
            }
            catch (Exception ex)
            {
                string error = "An error occurred while generating the client token.";
                _logger.LogError(ex, "Error in GetClientToken for plan {PlanId}", planId);
                return StatusCode(500, new { Error = error, Details = ex.Message });
            }
        }
    }

    public class PlanRequest
    {
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
    }

    public class SubscriptionRequest
    {
        public string PlanId { get; set; }
    }

    public class ReasonRequest
    {
        public string Reason { get; set; }
    }
} 