using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.CloverHostedCheckout.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.CloverHostedCheckout.Controllers
{
    public class PaymentCloverHostedCheckoutController : BasePaymentController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public PaymentCloverHostedCheckoutController(
            ILocalizationService localizationService,
            ILogger logger,
            INotificationService notificationService,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IPaymentPluginManager paymentPluginManager,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            IWebHelper webHelper,
            IWorkContext workContext)
        {
            _localizationService = localizationService;
            _logger = logger;
            _notificationService = notificationService;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _paymentPluginManager = paymentPluginManager;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            // Load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var cloverHostedCheckoutPaymentSettings = _settingService.LoadSetting<CloverHostedCheckoutPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                UseSandbox = cloverHostedCheckoutPaymentSettings.UseSandbox,
                ApiKey = cloverHostedCheckoutPaymentSettings.ApiKey,
                MerchantId = cloverHostedCheckoutPaymentSettings.MerchantId,
                AdditionalFee = cloverHostedCheckoutPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = cloverHostedCheckoutPaymentSettings.AdditionalFeePercentage,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope <= 0)
                return View("~/Plugins/Payments.CloverHostedCheckout/Views/Configure.cshtml", model);

            model.UseSandbox_OverrideForStore = _settingService.SettingExists(cloverHostedCheckoutPaymentSettings, x => x.UseSandbox, storeScope);
            model.ApiKey_OverrideForStore = _settingService.SettingExists(cloverHostedCheckoutPaymentSettings, x => x.ApiKey, storeScope);
            model.MerchantId_OverrideForStore = _settingService.SettingExists(cloverHostedCheckoutPaymentSettings, x => x.MerchantId, storeScope);
            model.AdditionalFee_OverrideForStore = _settingService.SettingExists(cloverHostedCheckoutPaymentSettings, x => x.AdditionalFee, storeScope);
            model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(cloverHostedCheckoutPaymentSettings, x => x.AdditionalFeePercentage, storeScope);

            return View("~/Plugins/Payments.CloverHostedCheckout/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            // Load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var cloverHostedCheckoutPaymentSettings = _settingService.LoadSetting<CloverHostedCheckoutPaymentSettings>(storeScope);

            // Save settings
            cloverHostedCheckoutPaymentSettings.UseSandbox = model.UseSandbox;
            cloverHostedCheckoutPaymentSettings.ApiKey = model.ApiKey;
            cloverHostedCheckoutPaymentSettings.MerchantId = model.MerchantId;
            cloverHostedCheckoutPaymentSettings.AdditionalFee = model.AdditionalFee;
            cloverHostedCheckoutPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(cloverHostedCheckoutPaymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(cloverHostedCheckoutPaymentSettings, x => x.ApiKey, model.ApiKey_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(cloverHostedCheckoutPaymentSettings, x => x.MerchantId, model.MerchantId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(cloverHostedCheckoutPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(cloverHostedCheckoutPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);

            // Now clear settings cache
            _settingService.ClearCache();

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        /// <summary>
        /// Handle return from Clover checkout
        /// </summary>
        public IActionResult Return()
        {
            var orderId = _webHelper.QueryString<string>("orderId");
            var paymentId = _webHelper.QueryString<string>("paymentId");
            var status = _webHelper.QueryString<string>("status");

            if (string.IsNullOrEmpty(orderId))
                return RedirectToAction("Index", "Home", new { area = string.Empty });

            Guid orderGuid;
            try
            {
                orderGuid = new Guid(orderId);
            }
            catch
            {
                return RedirectToAction("Index", "Home", new { area = string.Empty });
            }

            var order = _orderService.GetOrderByGuid(orderGuid);
            if (order == null)
                return RedirectToAction("Index", "Home", new { area = string.Empty });

            // Add order note
            var noteBuilder = new System.Text.StringBuilder();
            noteBuilder.AppendLine("Clover Hosted Checkout Return:");
            noteBuilder.AppendLine($"Payment ID: {paymentId}");
            noteBuilder.AppendLine($"Status: {status}");

            order.OrderNotes.Add(new OrderNote
            {
                Note = noteBuilder.ToString(),
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);

            // Process payment based on status
            if (!string.IsNullOrEmpty(status) && status.Equals("success", StringComparison.OrdinalIgnoreCase))
            {
                if (_orderProcessingService.CanMarkOrderAsPaid(order))
                {
                    order.AuthorizationTransactionId = paymentId;
                    _orderService.UpdateOrder(order);
                    _orderProcessingService.MarkOrderAsPaid(order);
                }
            }

            return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
        }

        /// <summary>
        /// Handle cancel from Clover checkout
        /// </summary>
        public IActionResult Cancel()
        {
            var order = _orderService.SearchOrders(_storeContext.CurrentStore.Id,
                customerId: _workContext.CurrentCustomer.Id, pageSize: 1).FirstOrDefault();

            if (order != null)
            {
                // Add order note
                order.OrderNotes.Add(new OrderNote
                {
                    Note = "Customer cancelled Clover Hosted Checkout payment",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);

                return RedirectToRoute("OrderDetails", new { orderId = order.Id });
            }

            return RedirectToRoute("Homepage");
        }

        /// <summary>
        /// Handle webhook notifications from Clover
        /// </summary>
        [HttpPost]
        public IActionResult Webhook()
        {
            try
            {
                // Read the webhook payload
                using (var reader = new System.IO.StreamReader(Request.Body))
                {
                    var webhookBody = reader.ReadToEnd();

                    // Log the webhook for debugging
                    _logger.Information($"Clover webhook received: {webhookBody}");

                    // Parse webhook and process accordingly
                    // This is a simplified version - you should validate the webhook signature in production
                    // and parse the JSON to get order and payment information

                    // Return 200 OK to acknowledge receipt
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Clover webhook processing error", ex);
                return BadRequest();
            }
        }

        #endregion
    }
}
