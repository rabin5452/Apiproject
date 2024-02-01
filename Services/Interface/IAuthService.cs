using Practiseproject.DTO;

namespace Practiseproject.Services.Interface
{
    public interface IAuthService
    {
        AddUser Add(AddUser user);
        string Login(LoginModel loginModel);
    }
}
