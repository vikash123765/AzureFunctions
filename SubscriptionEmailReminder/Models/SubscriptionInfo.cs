using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subsription5DaysExpiryNotification.Models
{
    public class SubscriptionInfo
    {
        public string SubscriptionType { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}