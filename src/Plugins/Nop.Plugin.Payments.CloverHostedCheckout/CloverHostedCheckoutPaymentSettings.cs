using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.CloverHostedCheckout
{
    /// <summary>
    /// Represents settings of the Clover Hosted Checkout payment plugin
    /// </summary>
    public class CloverHostedCheckoutPaymentSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether to use sandbox (testing environment)
        /// </summary>
        public bool UseSandbox { get; set; }

        /// <summary>
        /// Gets or sets the Clover API Key (Public Key)
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the Clover Merchant ID
        /// </summary>
        public string MerchantId { get; set; }

        /// <summary>
        /// Gets or sets an additional fee
        /// </summary>
        public decimal AdditionalFee { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
        /// </summary>
        public bool AdditionalFeePercentage { get; set; }
    }
}
