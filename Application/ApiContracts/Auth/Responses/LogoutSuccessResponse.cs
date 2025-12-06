
using Application;
using Application.ApiContracts;
using Application.ApiContracts.Auth;

namespace Application.ApiContracts.Auth.Responses
{
    public class LogoutSuccessResponse
    {
        public string Message { get; set; } = "Logout successful!";
    }
}
