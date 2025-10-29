using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using UserCenter.Service;

// 1. 创建构建器时，配置 Kestrel 服务器
var builder = WebApplication.CreateBuilder(args);

// 关键配置：强制指定端口使用 HTTP/2 协议
builder.WebHost.ConfigureKestrel(options =>
{
    // 监听所有IP的 5000 端口（HTTP），仅支持 HTTP/2
    options.ListenAnyIP(5001, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);

    // 若需启用 HTTPS（推荐生产环境），可添加以下配置（需替换证书路径和密码）
    // options.ListenAnyIP(5001, o =>
    // {
    //     o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    //     o.UseHttps("path/to/certificate.pfx", "certificate-password");
    // });
});

// 2. 添加 gRPC 服务（现有代码不变）
builder.Services.AddGrpc();

var app = builder.Build();

// 3. 映射 gRPC 服务（现有代码不变）
app.MapGrpcService<UserGrpcService>();

// 4. 启动应用（现有代码不变）
app.Run();

//var builder = WebApplication.CreateBuilder(args);

////添加gRPC服务
//builder.Services.AddGrpc();

//var app = builder.Build();

//app.MapGrpcService<UserGrpcService>();

////app.MapHealthChecks("/health");

//app.Run();