using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.CloverHostedCheckout.Infrastructure
{
    public partial class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="routeBuilder">Route builder</param>
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            // Return handler
            routeBuilder.MapRoute("Plugin.Payments.CloverHostedCheckout.Return", "Plugins/PaymentCloverHostedCheckout/Return",
                 new { controller = "PaymentCloverHostedCheckout", action = "Return" });

            // Cancel handler
            routeBuilder.MapRoute("Plugin.Payments.CloverHostedCheckout.Cancel", "Plugins/PaymentCloverHostedCheckout/Cancel",
                 new { controller = "PaymentCloverHostedCheckout", action = "Cancel" });

            // Webhook handler
            routeBuilder.MapRoute("Plugin.Payments.CloverHostedCheckout.Webhook", "Plugins/PaymentCloverHostedCheckout/Webhook",
                 new { controller = "PaymentCloverHostedCheckout", action = "Webhook" });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => -1;
    }
}
