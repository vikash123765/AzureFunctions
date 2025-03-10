using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subsription5DaysExpiryNotification.Models
{
    public class UserSubscriptionInfo
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<SubscriptionInfo> ExpiringSubscriptions { get; set; }
    }
}
