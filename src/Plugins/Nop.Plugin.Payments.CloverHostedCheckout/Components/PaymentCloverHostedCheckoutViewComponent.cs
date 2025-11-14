using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.CloverHostedCheckout.Components
{
    [ViewComponent(Name = "PaymentCloverHostedCheckout")]
    public class PaymentCloverHostedCheckoutViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.CloverHostedCheckout/Views/PaymentInfo.cshtml");
        }
    }
}
