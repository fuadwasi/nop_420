using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.CloverHostedCheckout.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Directory;

namespace Nop.Plugin.Payments.CloverHostedCheckout
{
    /// <summary>
    /// Clover Hosted Checkout payment processor
    /// </summary>
    public class CloverHostedCheckoutPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly CloverHostedCheckoutPaymentSettings _cloverHostedCheckoutPaymentSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly IPaymentService _paymentService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly CloverHostedCheckoutHttpClient _cloverHostedCheckoutHttpClient;

        #endregion

        #region Ctor

        public CloverHostedCheckoutPaymentProcessor(
            CloverHostedCheckoutPaymentSettings cloverHostedCheckoutPaymentSettings,
            IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            IPaymentService paymentService,
            ISettingService settingService,
            IWebHelper webHelper,
            ICurrencyService currencyService,
            CurrencySettings currencySettings,
            CloverHostedCheckoutHttpClient cloverHostedCheckoutHttpClient)
        {
            _cloverHostedCheckoutPaymentSettings = cloverHostedCheckoutPaymentSettings;
            _httpContextAccessor = httpContextAccessor;
            _localizationService = localizationService;
            _paymentService = paymentService;
            _settingService = settingService;
            _webHelper = webHelper;
            _currencyService = currencyService;
            _currencySettings = currencySettings;
            _cloverHostedCheckoutHttpClient = cloverHostedCheckoutHttpClient;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult();
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            // Get store location for return URLs
            var storeLocation = _webHelper.GetStoreLocation();

            // Get order details
            var order = postProcessPaymentRequest.Order;
            
            // Convert order total to cents (Clover API requires amount in cents)
            var amountInCents = (long)(Math.Round(order.OrderTotal, 2) * 100);

            // Get currency code
            var currency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
            var currencyCode = currency?.CurrencyCode ?? "USD";

            // Create checkout session
            var response = _cloverHostedCheckoutHttpClient.CreateCheckoutSession(
                _cloverHostedCheckoutPaymentSettings.ApiKey,
                _cloverHostedCheckoutPaymentSettings.MerchantId,
                amountInCents,
                currencyCode,
                order.OrderGuid.ToString(),
                _cloverHostedCheckoutPaymentSettings.UseSandbox
            );

            // Parse response to get checkout URL
            // For now, we'll construct the redirect URL based on Clover's hosted checkout pattern
            var baseUrl = _cloverHostedCheckoutPaymentSettings.UseSandbox
                ? "https://sandbox.dev.clover.com/invoicingcheckoutservice/v1/checkout/"
                : "https://checkout.clover.com/";

            // Construct the redirect URL with parameters
            var redirectUrl = $"{baseUrl}?mid={_cloverHostedCheckoutPaymentSettings.ApiKey}" +
                             $"&amount={amountInCents}" +
                             $"&currency={currencyCode}" +
                             $"&orderId={order.OrderGuid}" +
                             $"&returnUrl={storeLocation}Plugins/PaymentCloverHostedCheckout/Return" +
                             $"&cancelUrl={storeLocation}Plugins/PaymentCloverHostedCheckout/Cancel";

            _httpContextAccessor.HttpContext.Response.Redirect(redirectUrl);
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            // You can put any logic here
            // For example, hide this payment method if all products in the cart are downloadable
            // or hide this payment method if current customer is from certain country
            return false;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return _paymentService.CalculateAdditionalFee(cart,
                _cloverHostedCheckoutPaymentSettings.AdditionalFee, 
                _cloverHostedCheckoutPaymentSettings.AdditionalFeePercentage);
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            return new CapturePaymentResult { Errors = new[] { "Capture method not supported" } };
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            return new RefundPaymentResult { Errors = new[] { "Refund method not supported" } };
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            return new VoidPaymentResult { Errors = new[] { "Void method not supported" } };
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            // Let's ensure that at least 5 seconds passed after order is placed
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 5)
                return false;

            return true;
        }

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>List of validating errors</returns>
        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            return new List<string>();
        }

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>Payment info holder</returns>
        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            return new ProcessPaymentRequest();
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentCloverHostedCheckout/Configure";
        }

        /// <summary>
        /// Gets a name of a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <returns>View component name</returns>
        public string GetPublicViewComponentName()
        {
            return "PaymentCloverHostedCheckout";
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            // Settings
            _settingService.SaveSetting(new CloverHostedCheckoutPaymentSettings
            {
                UseSandbox = true
            });

            // Locales
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.CloverHostedCheckout.Fields.UseSandbox", "Use Sandbox");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.CloverHostedCheckout.Fields.UseSandbox.Hint", "Check to enable Sandbox (testing environment).");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.CloverHostedCheckout.Fields.ApiKey", "API Key");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.CloverHostedCheckout.Fields.ApiKey.Hint", "Enter your Clover API Key (Public Key).");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.CloverHostedCheckout.Fields.MerchantId", "Merchant ID");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.CloverHostedCheckout.Fields.MerchantId.Hint", "Enter your Clover Merchant ID.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.CloverHostedCheckout.Fields.AdditionalFee", "Additional fee");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.CloverHostedCheckout.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.CloverHostedCheckout.Fields.AdditionalFeePercentage", "Additional fee. Use percentage");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.CloverHostedCheckout.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.CloverHostedCheckout.Fields.RedirectionTip", "You will be redirected to Clover secure checkout to complete the payment.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.CloverHostedCheckout.PaymentMethodDescription", "Pay securely using Clover Hosted Checkout");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.CloverHostedCheckout.Instructions", 
                "<p><b>Clover Hosted Checkout Configuration</b><br />" +
                "1. Sign up for a Clover account at <a href=\"https://www.clover.com/\" target=\"_blank\">https://www.clover.com/</a><br />" +
                "2. Get your API credentials from the Clover dashboard<br />" +
                "3. Enter your Merchant ID and API Key below<br />" +
                "4. Enable sandbox mode for testing<br />" +
                "5. Clover Hosted Checkout supports multiple payment methods including credit/debit cards and digital wallets</p>");

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            // Settings
            _settingService.DeleteSetting<CloverHostedCheckoutPaymentSettings>();

            // Locales
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.CloverHostedCheckout.Fields.UseSandbox");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.CloverHostedCheckout.Fields.UseSandbox.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.CloverHostedCheckout.Fields.ApiKey");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.CloverHostedCheckout.Fields.ApiKey.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.CloverHostedCheckout.Fields.MerchantId");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.CloverHostedCheckout.Fields.MerchantId.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.CloverHostedCheckout.Fields.AdditionalFee");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.CloverHostedCheckout.Fields.AdditionalFee.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.CloverHostedCheckout.Fields.AdditionalFeePercentage");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.CloverHostedCheckout.Fields.AdditionalFeePercentage.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.CloverHostedCheckout.Fields.RedirectionTip");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.CloverHostedCheckout.PaymentMethodDescription");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.CloverHostedCheckout.Instructions");

            base.Uninstall();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture => false;

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund => false;

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund => false;

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid => false;

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo => false;

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public string PaymentMethodDescription => _localizationService.GetResource("Plugins.Payments.CloverHostedCheckout.PaymentMethodDescription");

        #endregion
    }
}
