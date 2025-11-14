using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace Nop.Plugin.Payments.CloverHostedCheckout.Services
{
    /// <summary>
    /// Represents the HTTP client to request Clover services
    /// </summary>
    public class CloverHostedCheckoutHttpClient
    {
        #region Fields

        private readonly HttpClient _httpClient;

        #endregion

        #region Ctor

        public CloverHostedCheckoutHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create a Clover checkout session
        /// </summary>
        /// <param name="apiKey">API key</param>
        /// <param name="merchantId">Merchant ID</param>
        /// <param name="amount">Amount in cents</param>
        /// <param name="currency">Currency code</param>
        /// <param name="orderId">Order ID</param>
        /// <param name="useSandbox">Use sandbox environment</param>
        /// <returns>Checkout session response</returns>
        public string CreateCheckoutSession(string apiKey, string merchantId, long amount, string currency, string orderId, bool useSandbox)
        {
            try
            {
                var baseUrl = useSandbox 
                    ? "https://sandbox.dev.clover.com/v3/merchants/" 
                    : "https://api.clover.com/v3/merchants/";

                var url = $"{baseUrl}{merchantId}/checkouts";

                var requestBody = new
                {
                    merchant = new { id = merchantId },
                    amount = amount,
                    currency = currency,
                    orderId = orderId
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var response = _httpClient.PostAsync(url, content).Result;
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                return $"{{\"error\": \"{ex.Message}\"}}";
            }
        }

        /// <summary>
        /// Get checkout session status
        /// </summary>
        /// <param name="apiKey">API key</param>
        /// <param name="merchantId">Merchant ID</param>
        /// <param name="checkoutId">Checkout ID</param>
        /// <param name="useSandbox">Use sandbox environment</param>
        /// <returns>Checkout status response</returns>
        public string GetCheckoutStatus(string apiKey, string merchantId, string checkoutId, bool useSandbox)
        {
            try
            {
                var baseUrl = useSandbox 
                    ? "https://sandbox.dev.clover.com/v3/merchants/" 
                    : "https://api.clover.com/v3/merchants/";

                var url = $"{baseUrl}{merchantId}/checkouts/{checkoutId}";

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var response = _httpClient.GetAsync(url).Result;
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                return $"{{\"error\": \"{ex.Message}\"}}";
            }
        }

        #endregion
    }
}
