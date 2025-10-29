using Grpc.Core;
using UserCenterService.Tools;

namespace UserCenter.Service;

public class UserGrpcService : UserCenterService.UserCenterServiceBase
{
    public override Task<RegisterResponse> Register(RegisterRequest registerReq, ServerCallContext context)
    {
        var user = MySQL.QueryData("UserBaseInfoTable", registerReq.Username);

        if (user != null)
        {
            return Task.FromResult(new RegisterResponse
            {
                Success = false,
                Msg = "用户已存在",
                PlayerId = user.PlayerId
            });
        }

        try
        {
            if (MySQL.AddDataTOTable("UserBaseInfoTable",
            registerReq.Username,
            registerReq.Password,
            out long userID))
            {
                Console.WriteLine($"{userID}注册成功！");
                return Task.FromResult(new RegisterResponse
                {
                    Success = true,
                    Msg = $"注册成功，你的ID为{userID}",
                    PlayerId = userID
                });
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"注册出错了>_<:{e.Message}");
        }
        return Task.FromResult(new RegisterResponse
        {
            Success = false,
            Msg = "注册失败"
        });
    }
    public override Task<LoginResponse> Login(LoginRequest loginReq, ServerCallContext context)
    {
        var user = MySQL.QueryData("UserBaseInfoTable", loginReq.PlayerId);
        if (user == null)
            user = MySQL.QueryData("UserBaseInfoTable", loginReq.Username);

        if (user == null)
        {
            return Task.FromResult(new LoginResponse
            {
                Success = false,
                Msg = "用户不存在"
            });
        }

        string encryptedPwd = Tools.ComputMd5Hash(loginReq.Password);
        if (encryptedPwd.Equals(user.Password))
        {
            return Task.FromResult(new LoginResponse
            {
                Success = false,
                Msg = "用户名或密码错误"
            });
        }

        //string token = Tools.GenerateJWToken(user.PlayerId.ToString());
        long expireTime = DateTimeOffset.UtcNow.Add(Tools._tokenExpire).ToUnixTimeMilliseconds();

        return Task.FromResult(new LoginResponse
        {
            Success = true,
            Msg = "登陆成功",
            //Token = token,
            ExpireTime = expireTime
        });
    }
    public override Task<GetUserInfoResponse> ChangeInfo(ChangeInfoRequest request, ServerCallContext context)
    {
        return base.ChangeInfo(request, context);
    }
    public override Task<GetUserInfoResponse> GetUserInfo(GetUserInfoRequest request, ServerCallContext context)
    {
        return base.GetUserInfo(request, context);
    }
}