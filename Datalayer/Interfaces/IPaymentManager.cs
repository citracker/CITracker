using Shared.DTO;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datalayer.Interfaces
{
    public interface IPaymentManager
    {
        Task<ResponseHandler<PaymentProvider>> FetchPaymentOptions();
    }
}
