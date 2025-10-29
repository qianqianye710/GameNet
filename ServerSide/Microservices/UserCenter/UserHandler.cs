using UserCenter;
using UserCenter.Client;

namespace ServerSide.Microservices.UserCenter
{
    internal class LoginHandler
    {
        public static async Task<LoginResponse> CheckLoginInfo(LoginRequest loginReq)
        {
            return await _CheckLoginInfo(loginReq);
        }
        private static async Task<LoginResponse> _CheckLoginInfo(LoginRequest loginReq)
        {
            var userCenterClient = new UserGrpcClient("http://localhost:5001");

            return await userCenterClient.LoginAsync(loginReq.Username, loginReq.Password);
        }
    }
    internal class RegisterHandler
    {
        public static async Task<RegisterResponse> RegisterUser(RegisterRequest registerReq)
        {
            return await _RegisterUser(registerReq);
        }
        private static async Task<RegisterResponse> _RegisterUser(RegisterRequest registerReq)
        {
            var userCenterClient = new UserGrpcClient("http://localhost:5001");

            return await userCenterClient.RegisterAsync(registerReq.Username, registerReq.Password);
        }
    }
}
