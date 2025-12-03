using FitnessApp.Domain.Entities;
using FitnessApp.Domain.Interfaces;
using FitnessApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace FitnessApp.Infrastructure.Services;

/// <summary>
/// Stripe payment service implementation
/// </summary>
public class StripePaymentService : IPaymentService
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StripePaymentService> _logger;

    public StripePaymentService(
        IConfiguration configuration,
        ApplicationDbContext context,
        ILogger<StripePaymentService> logger)
    {
        _configuration = configuration;
        _context = context;
        _logger = logger;
        
        // Configure Stripe API key
        StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
    }

    public async Task<string> CreateCustomerAsync(string userId, string email, string name)
    {
        try
        {
            var options = new CustomerCreateOptions
            {
                Email = email,
                Name = name,
                Metadata = new Dictionary<string, string>
                {
                    { "userId", userId }
                }
            };

            var service = new CustomerService();
            var customer = await service.CreateAsync(options);
            
            _logger.LogInformation("Created Stripe customer {CustomerId} for user {UserId}", customer.Id, userId);
            
            return customer.Id;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to create Stripe customer for user {UserId}", userId);
            throw;
        }
    }

    public async Task<string> CreateSubscriptionCheckoutSessionAsync(
        string customerId,
        string priceId,
        string successUrl,
        string cancelUrl)
    {
        try
        {
            var options = new SessionCreateOptions
            {
                Customer = customerId,
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = priceId,
                        Quantity = 1,
                    }
                },
                Mode = "subscription",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);
            
            _logger.LogInformation("Created checkout session {SessionId} for customer {CustomerId}", session.Id, customerId);
            
            return session.Url;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to create checkout session for customer {CustomerId}", customerId);
            throw;
        }
    }

    public async Task<bool> CancelSubscriptionAsync(string subscriptionId)
    {
        try
        {
            var service = new SubscriptionService();
            var subscription = await service.CancelAsync(subscriptionId);
            
            _logger.LogInformation("Cancelled subscription {SubscriptionId}", subscriptionId);
            
            return subscription.Status == "canceled";
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to cancel subscription {SubscriptionId}", subscriptionId);
            throw;
        }
    }

    public async Task<SubscriptionDetails?> GetSubscriptionDetailsAsync(string subscriptionId)
    {
        try
        {
            var service = new SubscriptionService();
            var subscription = await service.GetAsync(subscriptionId);
            
            return new SubscriptionDetails
            {
                SubscriptionId = subscription.Id,
                CustomerId = subscription.CustomerId,
                Status = subscription.Status,
                CurrentPeriodStart = subscription.CurrentPeriodStart,
                CurrentPeriodEnd = subscription.CurrentPeriodEnd,
                PriceId = subscription.Items.Data.FirstOrDefault()?.Price?.Id ?? string.Empty
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to get subscription details for {SubscriptionId}", subscriptionId);
            return null;
        }
    }

    public async Task ProcessWebhookEventAsync(string json, string signature)
    {
        try
        {
            var webhookSecret = _configuration["Stripe:WebhookSecret"];
            var stripeEvent = EventUtility.ConstructEvent(json, signature, webhookSecret);

            _logger.LogInformation("Processing Stripe webhook event: {EventType}", stripeEvent.Type);

            switch (stripeEvent.Type)
            {
                case "checkout.session.completed":
                    await HandleCheckoutSessionCompleted(stripeEvent);
                    break;
                    
                case "customer.subscription.created":
                case "customer.subscription.updated":
                    await HandleSubscriptionUpdated(stripeEvent);
                    break;
                    
                case "customer.subscription.deleted":
                    await HandleSubscriptionDeleted(stripeEvent);
                    break;
                    
                case "invoice.paid":
                    await HandleInvoicePaid(stripeEvent);
                    break;
                    
                case "invoice.payment_failed":
                    await HandlePaymentFailed(stripeEvent);
                    break;
                    
                default:
                    _logger.LogInformation("Unhandled event type: {EventType}", stripeEvent.Type);
                    break;
            }
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to process webhook event");
            throw;
        }
    }

    private async Task HandleCheckoutSessionCompleted(Event stripeEvent)
    {
        var session = stripeEvent.Data.Object as Session;
        if (session == null) return;

        _logger.LogInformation("Checkout session completed: {SessionId}", session.Id);
        
        // The subscription is created automatically by Stripe
        // We'll handle the actual subscription creation in HandleSubscriptionUpdated
    }

    private async Task HandleSubscriptionUpdated(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Subscription;
        if (subscription == null) return;

        var existingSubscription = await _context.UserSubscriptions
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == subscription.Id);

        if (existingSubscription != null)
        {
            // Update existing subscription
            existingSubscription.StartDate = subscription.CurrentPeriodStart;
            existingSubscription.EndDate = subscription.CurrentPeriodEnd;
            existingSubscription.Status = MapStripeStatus(subscription.Status);
            existingSubscription.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated subscription {SubscriptionId}", subscription.Id);
        }
        else
        {
            // Find user by Stripe customer ID
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.StripeCustomerId == subscription.CustomerId);
                
            if (user != null)
            {
                // Find plan by Stripe price ID
                var priceId = subscription.Items.Data.FirstOrDefault()?.Price?.Id;
                var plan = await _context.SubscriptionPlans
                    .FirstOrDefaultAsync(p => p.StripePriceId == priceId);
                    
                if (plan != null)
                {
                    var newSubscription = new UserSubscription
                    {
                        UserId = user.Id,
                        PlanId = plan.Id,
                        StripeSubscriptionId = subscription.Id,
                        StripeCustomerId = subscription.CustomerId,
                        StartDate = subscription.CurrentPeriodStart,
                        EndDate = subscription.CurrentPeriodEnd,
                        Status = MapStripeStatus(subscription.Status),
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    _context.UserSubscriptions.Add(newSubscription);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Created new subscription for user {UserId}", user.Id);
                }
            }
        }
    }

    private async Task HandleSubscriptionDeleted(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Subscription;
        if (subscription == null) return;

        var existingSubscription = await _context.UserSubscriptions
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == subscription.Id);

        if (existingSubscription != null)
        {
            existingSubscription.Status = SubscriptionStatus.Cancelled;
            existingSubscription.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            _logger.LogInformation("Cancelled subscription {SubscriptionId}", subscription.Id);
        }
    }

    private async Task HandleInvoicePaid(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        if (invoice == null) return;

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.StripeCustomerId == invoice.CustomerId);
            
        if (user != null)
        {
            var subscription = await _context.UserSubscriptions
                .FirstOrDefaultAsync(s => s.StripeSubscriptionId == invoice.SubscriptionId);
                
            var transaction = new PaymentTransaction
            {
                UserId = user.Id,
                SubscriptionId = subscription?.Id,
                Amount = invoice.AmountPaid / 100m, // Stripe amounts are in cents
                Currency = invoice.Currency.ToUpper(),
                Status = PaymentStatus.Succeeded,
                Type = PaymentType.SubscriptionPayment,
                StripeInvoiceId = invoice.Id,
                StripePaymentIntentId = invoice.PaymentIntentId,
                CompletedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.PaymentTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Recorded payment for invoice {InvoiceId}", invoice.Id);
        }
    }

    private async Task HandlePaymentFailed(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        if (invoice == null) return;

        var subscription = await _context.UserSubscriptions
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == invoice.SubscriptionId);

        if (subscription != null)
        {
            subscription.Status = SubscriptionStatus.PastDue;
            subscription.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            _logger.LogWarning("Payment failed for subscription {SubscriptionId}", invoice.SubscriptionId);
        }
    }

    private static SubscriptionStatus MapStripeStatus(string stripeStatus)
    {
        return stripeStatus switch
        {
            "active" => SubscriptionStatus.Active,
            "canceled" => SubscriptionStatus.Cancelled,
            "past_due" => SubscriptionStatus.PastDue,
            "paused" => SubscriptionStatus.Paused,
            _ => SubscriptionStatus.Active
        };
    }
}
