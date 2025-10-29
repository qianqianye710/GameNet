using Grpc.Net.Client;
using UserCenter;

namespace UserCenter.Client;

public class UserGrpcClient
{
    public static UserCenterService.UserCenterServiceClient _client { get; private set; }

    //传入服务器地址
    public UserGrpcClient(string serviceUr1)
    {
        var channel = GrpcChannel.ForAddress(serviceUr1);
        _client = new UserCenterService.UserCenterServiceClient(channel);
    }

    public async Task<LoginResponse> LoginAsync(string userName,string password)
    {
        try
        {
            var request = new LoginRequest
            {
                Username = userName,
                Password = password
            };

            return await _client.LoginAsync(request);
        }
        catch(Exception e)
        {
            Console.WriteLine("微服务出现故障:" + e.Message);
            return null;
        }
    }
    public async Task<RegisterResponse> RegisterAsync(string userName,string password)
    {
        try
        {
            var request = new RegisterRequest
            {
                Username = userName,
                Password = password
            };

            return await _client.RegisterAsync(request);
        }
        catch(Exception e)
        {
            Console.WriteLine("微服务出现故障:" + e.Message);
            return null;
        }
    }
}