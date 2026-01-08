using Shared.DTO;
using Shared.Models;

namespace Datalayer.Interfaces
{
    public interface IUserManager
    {
        Task<ResponseHandler<CIUserDTO>> GetUserByEmail(string email);
    }
}
