using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ViewModels
{
    public class CheckoutVM
    { 
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public List<PaymentProvider> PaymentProvider { get; set; }
        public List<Country> Country { get; set; }
        public SubscriptionPlan SubscriptionPlan { get; set; }
    }
}
