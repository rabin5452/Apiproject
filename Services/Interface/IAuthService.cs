using Practiseproject.DTO;

namespace Practiseproject.Services.Interface
{
    public interface IAuthService
    {
        Task<AddUser> AddAdmin(AddUser adduser);
        Task<AddUser> Add(AddUser adduser);
        Task<TokenResponse> Login(LoginModel loginModel);
        Task<TokenModel> RefreshToken(TokenModel tokenModel);
    }
}
