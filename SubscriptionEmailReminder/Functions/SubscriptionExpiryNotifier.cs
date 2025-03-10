using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Subscription5DaysExpiryNotification.Services;
using Subsription5DaysExpiryNotification.Models;

namespace Subsription5DaysExpiryNotification.Functions
{
    public class SubscriptionExpiryNotifier
    {
        private readonly HttpClient _httpClient;
        private readonly EmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public SubscriptionExpiryNotifier(HttpClient httpClient, IConfiguration configuration, EmailSender emailSender)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));

            Console.WriteLine($"🔍 Debug: ExpiringSubscriptionsApiUrl = {_configuration["ExpiringSubscriptionsApiUrl"]}");
            Console.WriteLine($"🔍 Debug: SmtpServer = {_configuration["SmtpServer"]}");
            Console.WriteLine($"🔍 Debug: SenderEmail = {_configuration["SenderEmail"]}");
        }

        [Function("SubscriptionExpiryNotifier")]  // ✅ Function Attribute
        public async Task Run(
            [TimerTrigger("0 0 0 * * *")] TimerInfo myTimer,  // ✅ Runs every day at midnight
            FunctionContext context)
        {
            var logger = context.GetLogger<SubscriptionExpiryNotifier>();
            Console.WriteLine($"🔵 Azure Function triggered at: {DateTime.UtcNow}");

            if (string.IsNullOrEmpty(_configuration["ExpiringSubscriptionsApiUrl"]))
            {
                Console.WriteLine("❌ ERROR: ExpiringSubscriptionsApiUrl is missing!");
                throw new ArgumentNullException("ExpiringSubscriptionsApiUrl");
            }

            string apiUrl = _configuration["ExpiringSubscriptionsApiUrl"];

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                var expiringSubscriptions = JsonSerializer.Deserialize<List<UserSubscriptionInfo>>(jsonResponse);

                if (expiringSubscriptions != null && expiringSubscriptions.Count > 0)
                {
                    foreach (var user in expiringSubscriptions)
                    {
                        string emailBody = EmailTemplate(user.UserName, user.ExpiringSubscriptions);
                        await _emailSender.SendEmailAsync(user.Email, "Your Subscription is Expiring Soon!", emailBody);
                    }
                    Console.WriteLine("✅ Emails sent successfully!");
                }
                else
                {
                    Console.WriteLine("ℹ️ No expiring subscriptions found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error calling API: {ex.Message}");
            }
        }

        private static string EmailTemplate(string userName, List<SubscriptionInfo> subscriptions)
        {
            string subscriptionList = string.Join("", subscriptions.Select(sub =>
                $"<li>{sub.SubscriptionType} - Expires on {sub.ExpiryDate:MMMM dd, yyyy}</li>"));

            return $@"
                <html>
                <body>
                    <h2>Dear {userName},</h2>
                    <p>Your subscription(s) are expiring soon:</p>
                    <ul>
                        {subscriptionList}
                    </ul>
                    <p>Please renew your subscription to continue enjoying our services.</p>
                    <p>Best regards, <br> Vikash News Site</p>
                </body>
                </html>
            ";
        }
    }
}
