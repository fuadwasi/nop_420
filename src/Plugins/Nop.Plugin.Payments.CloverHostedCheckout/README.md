# Clover Hosted Checkout Payment Plugin for nopCommerce 4.20

## Overview
This plugin integrates Clover Hosted Checkout with nopCommerce 4.20, enabling merchants to accept payments through Clover's secure hosted payment page. The plugin supports multiple payment methods including credit/debit cards and digital wallets.

## Features
- **Secure Payment Processing**: Redirect customers to Clover's PCI-DSS compliant hosted checkout
- **Multiple Payment Methods**: Support for credit/debit cards and digital wallets
- **Sandbox & Production Modes**: Test in sandbox before going live
- **Configurable Fees**: Add fixed or percentage-based additional fees
- **Order Tracking**: Payment status reflected in nopCommerce order history
- **Webhook Support**: Real-time payment status updates
- **Localization Ready**: Supports multi-language stores

## Installation

1. Copy the plugin folder to `~/Plugins/Nop.Plugin.Payments.CloverHostedCheckout`
2. Restart the application or rebuild the solution
3. Navigate to Admin > Configuration > Local Plugins
4. Find "Clover Hosted Checkout" and click Install
5. Click Configure to set up your Clover credentials

## Configuration

### Required Settings
1. **Merchant ID**: Your Clover Merchant ID (found in Clover dashboard)
2. **API Key**: Your Clover API Public Key (from Clover dashboard)
3. **Use Sandbox**: Enable for testing with Clover's sandbox environment

### Optional Settings
1. **Additional Fee**: Fixed amount or percentage to charge customers
2. **Additional Fee Percentage**: Toggle between fixed amount and percentage

### Getting Clover Credentials
1. Sign up for a Clover account at https://www.clover.com/
2. Log in to the Clover dashboard
3. Navigate to Account & Setup > API Tokens
4. Generate a new API token with appropriate permissions
5. Note your Merchant ID from your account settings

## Technical Details

### Architecture
- **Payment Method Type**: Redirection
- **Supported Operations**: Payment processing (authorize and capture)
- **Not Supported**: Refunds, voids, captures (must be done through Clover dashboard)
- **Recurring Payments**: Not supported

### Security & Compliance
- **PCI-DSS Compliant**: No sensitive payment data stored in nopCommerce
- **Secure Redirect**: Customers redirected to Clover's secure hosted checkout
- **Data Protection**: Only order reference and amount sent to Clover
- **Webhook Validation**: Includes webhook endpoint for payment notifications

### Flow
1. Customer completes checkout in nopCommerce
2. Order created with pending payment status
3. Customer redirected to Clover Hosted Checkout
4. Customer completes payment on Clover's secure page
5. Clover redirects back to nopCommerce with payment result
6. Order status updated based on payment result
7. Customer sees order confirmation

### Webhooks
The plugin includes a webhook endpoint at:
```
https://yourstore.com/Plugins/PaymentCloverHostedCheckout/Webhook
```

Configure this URL in your Clover dashboard to receive real-time payment notifications.

## Compatibility
- nopCommerce Version: 4.20
- .NET Core: 2.2
- No additional NuGet dependencies

## Code Structure
```
Nop.Plugin.Payments.CloverHostedCheckout/
├── CloverHostedCheckoutPaymentProcessor.cs  # Main payment processor
├── CloverHostedCheckoutPaymentSettings.cs   # Settings model
├── Components/
│   └── PaymentCloverHostedCheckoutViewComponent.cs
├── Controllers/
│   └── PaymentCloverHostedCheckoutController.cs  # Admin config + webhooks
├── Infrastructure/
│   ├── NopStartup.cs              # Dependency injection
│   └── RouteProvider.cs           # Route registration
├── Models/
│   └── ConfigurationModel.cs      # Admin configuration model
├── Services/
│   └── CloverHostedCheckoutHttpClient.cs  # API client
├── Views/
│   ├── Configure.cshtml           # Admin configuration page
│   ├── PaymentInfo.cshtml         # Customer payment info
│   └── _ViewImports.cshtml
├── logo.jpg
└── plugin.json
```

## Troubleshooting

### Plugin doesn't appear in Local Plugins
- Ensure the plugin is in the correct directory
- Restart the application
- Check Application Insights for any errors

### Payment redirect fails
- Verify Merchant ID and API Key are correct
- Check if using correct sandbox/production mode
- Ensure return URLs are accessible

### Payment status not updating
- Check webhook configuration in Clover dashboard
- Verify webhook URL is publicly accessible
- Review order notes for payment information

## Support
For issues related to:
- **Plugin functionality**: Check nopCommerce forums or create an issue
- **Clover API**: Contact Clover support at https://www.clover.com/support
- **Payment processing**: Review Clover documentation at https://docs.clover.com/

## License
This plugin follows the nopCommerce Public License.

## Changelog

### Version 1.00
- Initial release
- Support for Clover Hosted Checkout integration
- Admin configuration interface
- Webhook support for payment notifications
- Sandbox and production mode support
- Additional fee configuration
