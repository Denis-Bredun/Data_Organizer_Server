using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Data_Organizer_Server.Interfaces;
using Data_Organizer_Server.Models;

namespace Data_Organizer_Server.Services
{
    public class PayPalService : IPayPalService
    {
        private readonly HttpClient _httpClient;
        private readonly string _clientId;
        private readonly string _secretKey;
        private string _accessToken;
        private DateTime _tokenExpiry = DateTime.MinValue;

        public const string URL = "https://api-m.sandbox.paypal.com";

        public PayPalService()
        {
            _httpClient = new HttpClient();
            _clientId = Environment.GetEnvironmentVariable("PAYPAL_CLIENT_ID");
            _secretKey = Environment.GetEnvironmentVariable("PAYPAL_SECRET_KEY");
        }

        public async Task<string> GetAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken) && _tokenExpiry > DateTime.UtcNow)
            {
                return _accessToken;
            }

            var requestUrl = $"{URL}/v1/oauth2/token";
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
                {
                    Content = content
                };

                var authenticationString = $"{_clientId}:{_secretKey}";
                var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(authenticationString));
                
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                using var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"PayPal API error: {(int)response.StatusCode} - {errorResponse}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<PayPalTokenResponse>(responseContent);

                if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
                    throw new NullReferenceException("Received token from PayPal is empty!");

                _accessToken = tokenResponse.AccessToken;
                _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 300);

                return _accessToken;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Error during the request to PayPal API!", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception("Error deserializing the PayPal response!", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unknown error in PayPalService!", ex);
            }
        }

        private async Task<HttpRequestMessage> CreateAuthorizedRequestAsync(HttpMethod method, string url, object content = null)
        {
            var accessToken = await GetAccessTokenAsync();
            var request = new HttpRequestMessage(method, url);
            
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            
            if (content != null)
            {
                var jsonContent = JsonSerializer.Serialize(content);
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            }
            
            return request;
        }

        public async Task<string> CreatePlanAsync(string productName, string description, decimal price)
        {
            try
            {
                var productUrl = $"{URL}/v1/catalogs/products";
                var productRequest = await CreateAuthorizedRequestAsync(HttpMethod.Post, productUrl, new
                {
                    name = productName,
                    description = description,
                    type = "SERVICE",
                    category = "SOFTWARE"
                });

                using var productResponse = await _httpClient.SendAsync(productRequest);
                
                if (!productResponse.IsSuccessStatusCode)
                {
                    var errorResponse = await productResponse.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"PayPal Product API error: {(int)productResponse.StatusCode} - {errorResponse}");
                }

                var productResponseContent = await productResponse.Content.ReadAsStringAsync();
                var productData = JsonDocument.Parse(productResponseContent);
                var productId = productData.RootElement.GetProperty("id").GetString();

                var planUrl = $"{URL}/v1/billing/plans";
                var planRequest = await CreateAuthorizedRequestAsync(HttpMethod.Post, planUrl, new
                {
                    product_id = productId,
                    name = productName + " Monthly Plan",
                    description = "Monthly subscription for " + description,
                    status = "ACTIVE",
                    billing_cycles = new[]
                    {
                        new
                        {
                            frequency = new
                            {
                                interval_unit = "MONTH",
                                interval_count = 1
                            },
                            tenure_type = "REGULAR",
                            sequence = 1,
                            total_cycles = 0,
                            pricing_scheme = new
                            {
                                fixed_price = new
                                {
                                    value = price.ToString("0.00"),
                                    currency_code = "USD"
                                }
                            }
                        }
                    },
                    payment_preferences = new
                    {
                        auto_bill_outstanding = true,
                        setup_fee = new
                        {
                            value = "0",
                            currency_code = "USD"
                        },
                        setup_fee_failure_action = "CONTINUE",
                        payment_failure_threshold = 3
                    }
                });

                using var planResponse = await _httpClient.SendAsync(planRequest);
                
                if (!planResponse.IsSuccessStatusCode)
                {
                    var errorResponse = await planResponse.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"PayPal Plan API error: {(int)planResponse.StatusCode} - {errorResponse}");
                }

                var planResponseContent = await planResponse.Content.ReadAsStringAsync();
                var planData = JsonDocument.Parse(planResponseContent);
                var planId = planData.RootElement.GetProperty("id").GetString();

                return planId;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Error during the request to PayPal API!", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception("Error deserializing the PayPal response!", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unknown error in PayPalService.CreatePlanAsync!", ex);
            }
        }

        public async Task<object> GetPlanDetailsAsync(string planId)
        {
            try
            {
                var url = $"{URL}/v1/billing/plans/{planId}";
                var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, url);

                using var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"PayPal API error: {(int)response.StatusCode} - {errorResponse}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var plan = JsonSerializer.Deserialize<PayPalPlan>(responseContent);

                return plan;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Error getting plan details for {planId}!", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception("Error deserializing the PayPal response!", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unknown error in PayPalService.GetPlanDetailsAsync!", ex);
            }
        }

        public async Task<bool> ActivatePlanAsync(string planId)
        {
            try
            {
                var url = $"{URL}/v1/billing/plans/{planId}/activate";
                var request = await CreateAuthorizedRequestAsync(HttpMethod.Post, url);

                using var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"PayPal API error: {(int)response.StatusCode} - {errorResponse}");
                }

                return true;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Error activating plan {planId}!", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unknown error in PayPalService.ActivatePlanAsync!", ex);
            }
        }

        public async Task<bool> DeactivatePlanAsync(string planId)
        {
            try
            {
                var url = $"{URL}/v1/billing/plans/{planId}/deactivate";
                var request = await CreateAuthorizedRequestAsync(HttpMethod.Post, url);

                using var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"PayPal API error: {(int)response.StatusCode} - {errorResponse}");
                }

                return true;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Error deactivating plan {planId}!", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unknown error in PayPalService.DeactivatePlanAsync!", ex);
            }
        }

        public async Task<string> CreateSubscriptionAsync(string planId)
        {
            try
            {
                var url = $"{URL}/v1/billing/subscriptions";
                var request = await CreateAuthorizedRequestAsync(HttpMethod.Post, url, new
                {
                    plan_id = planId,
                    application_context = new
                    {
                        return_url = "https://example.com/return",
                        cancel_url = "https://example.com/cancel",
                        user_action = "SUBSCRIBE_NOW",
                        payment_method = new
                        {
                            payer_selected = "PAYPAL",
                            payee_preferred = "IMMEDIATE_PAYMENT_REQUIRED"
                        }
                    }
                });

                using var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"PayPal API error: {(int)response.StatusCode} - {errorResponse}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var subscriptionData = JsonDocument.Parse(responseContent);
                
                var subscriptionId = subscriptionData.RootElement.GetProperty("id").GetString();
                var links = subscriptionData.RootElement.GetProperty("links").EnumerateArray();
                string approvalUrl = null;
                
                foreach (var link in links)
                {
                    if (link.GetProperty("rel").GetString() == "approve")
                    {
                        approvalUrl = link.GetProperty("href").GetString();
                        break;
                    }
                }

                return JsonSerializer.Serialize(new { id = subscriptionId, approval_url = approvalUrl });
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Error creating subscription for plan {planId}!", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception("Error deserializing the PayPal response!", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unknown error in PayPalService.CreateSubscriptionAsync!", ex);
            }
        }

        public async Task<object> GetSubscriptionDetailsAsync(string subscriptionId)
        {
            try
            {
                var url = $"{URL}/v1/billing/subscriptions/{subscriptionId}";
                var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, url);

                using var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"PayPal API error: {(int)response.StatusCode} - {errorResponse}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var subscription = JsonSerializer.Deserialize<PayPalSubscription>(responseContent);

                return subscription;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Error getting subscription details for {subscriptionId}!", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception("Error deserializing the PayPal response!", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unknown error in PayPalService.GetSubscriptionDetailsAsync!", ex);
            }
        }

        public async Task<bool> SuspendSubscriptionAsync(string subscriptionId, string reason)
        {
            try
            {
                var url = $"{URL}/v1/billing/subscriptions/{subscriptionId}/suspend";
                var request = await CreateAuthorizedRequestAsync(HttpMethod.Post, url, new
                {
                    reason = reason
                });

                using var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"PayPal API error: {(int)response.StatusCode} - {errorResponse}");
                }

                return true;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Error suspending subscription {subscriptionId}!", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unknown error in PayPalService.SuspendSubscriptionAsync!", ex);
            }
        }

        public async Task<bool> CancelSubscriptionAsync(string subscriptionId, string reason)
        {
            try
            {
                var url = $"{URL}/v1/billing/subscriptions/{subscriptionId}/cancel";
                var request = await CreateAuthorizedRequestAsync(HttpMethod.Post, url, new
                {
                    reason = reason
                });

                using var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"PayPal API error: {(int)response.StatusCode} - {errorResponse}");
                }

                return true;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Error cancelling subscription {subscriptionId}!", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unknown error in PayPalService.CancelSubscriptionAsync!", ex);
            }
        }

        public async Task<bool> ActivateSubscriptionAsync(string subscriptionId, string reason)
        {
            try
            {
                var url = $"{URL}/v1/billing/subscriptions/{subscriptionId}/activate";
                var request = await CreateAuthorizedRequestAsync(HttpMethod.Post, url, new
                {
                    reason = reason
                });

                using var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"PayPal API error: {(int)response.StatusCode} - {errorResponse}");
                }

                return true;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Error activating subscription {subscriptionId}!", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unknown error in PayPalService.ActivateSubscriptionAsync!", ex);
            }
        }

        public async Task<bool> CapturePaymentAsync(string subscriptionId)
        {
            try
            {
                var url = $"{URL}/v1/billing/subscriptions/{subscriptionId}/capture";
                var request = await CreateAuthorizedRequestAsync(HttpMethod.Post, url, new
                {
                    note = "Captured payment",
                    capture_type = "OUTSTANDING_BALANCE"
                });

                using var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"PayPal API error: {(int)response.StatusCode} - {errorResponse}");
                }

                return true;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Error capturing payment for subscription {subscriptionId}!", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unknown error in PayPalService.CapturePaymentAsync!", ex);
            }
        }

        public async Task<List<object>> ListTransactionsAsync(string subscriptionId, string startDate, string endDate)
        {
            try
            {
                var url = $"{URL}/v1/billing/subscriptions/{subscriptionId}/transactions?start_time={startDate}&end_time={endDate}";
                var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, url);

                using var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"PayPal API error: {(int)response.StatusCode} - {errorResponse}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var transactionsResponse = JsonDocument.Parse(responseContent);
                
                var transactions = new List<object>();
                if (transactionsResponse.RootElement.TryGetProperty("transactions", out var transactionsElement))
                {
                    foreach (var transaction in transactionsElement.EnumerateArray())
                    {
                        var transactionObj = JsonSerializer.Deserialize<PayPalTransaction>(transaction.GetRawText());
                        transactions.Add(transactionObj);
                    }
                }

                return transactions;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Error listing transactions for subscription {subscriptionId}!", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception("Error deserializing the PayPal response!", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unknown error in PayPalService.ListTransactionsAsync!", ex);
            }
        }

        public async Task<string> GenerateClientTokenAsync(string planId)
        {
            try
            {
                await GetPlanDetailsAsync(planId);

                var clientConfig = new
                {
                    client_id = _clientId,
                    plan_id = planId,
                    timestamp = DateTime.UtcNow.ToString("o"),
                    api_url = URL
                };

                return JsonSerializer.Serialize(clientConfig);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating client token for plan {planId}!", ex);
            }
        }
    }
}
