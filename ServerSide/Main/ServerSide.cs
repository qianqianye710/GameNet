using System.Net;
using System.Net.Sockets;
using Google.Protobuf;
using ServerSide.Microservices.UserCenter;
using UserCenter;

namespace ServerSide.Main
{
    /// <summary>
    /// 服务器端
    /// </summary>
    internal class Server
    {
        private static TcpListener listener = null!;
        internal static List<ClientSession> sessions = new();

        internal static object _lock = new object();

        /// <summary>
        /// 主程序
        /// </summary>
        static void Main()
        {
            listener = new TcpListener(IPAddress.Any, 1220);
            listener.Start();
            Console.WriteLine("服务端启动，端口号:1220");

            while (true)
            {
                //阻塞等待客户端连接
                TcpClient client = listener.AcceptTcpClient();
                lock (_lock)
                {
                    //创建客户端对话对象，传入client和消息处理方法
                    sessions.Add(new ClientSession(client, HandleMsg));
                }
                //输出客户端连接信息，当前在线客户端数量
                Console.WriteLine($"客户端:{client}已连接，当前在线：{sessions.Count}");
            }
        }

        /// <summary>
        /// 处理客户端消息
        /// </summary>
        /// <param name="session">发送消息的客户端</param>
        /// <param name="msgType">消息类型</param>
        /// <param name="data">消息体</param>
        private static async void HandleMsg(ClientSession session,MsgType msgType, byte[] data)
        {
            switch (msgType)
            {
                //登录消息
                case MsgType.LoginReq:
                    //反序列化消息为LoginRequest对象
                    var loginReq = LoginRequest.Parser.ParseFrom(data);
                    //创建登录响应对象
                    var loginRes = await LoginHandler.CheckLoginInfo(loginReq);
                    Console.WriteLine(loginRes.Msg);
                    loginRes.Msg = loginRes.Success ? "Login successful!" : "Incorrect password!";
                    //给客户端发送登录结果消息
                    await session.SendMsgAsync(MsgType.LoginRsp, loginRes);
                    break;
                //注册消息
                case MsgType.RegisterReq:
                    var registerReq = RegisterRequest.Parser.ParseFrom(data);
                    var registerRes = await RegisterHandler.RegisterUser(registerReq);
                    Console.WriteLine(registerRes.Msg);
                    registerRes.Msg = registerRes.Success ? "Register success!" : "There is uesr!";
                    await session.SendMsgAsync(MsgType.LoginRsp, registerRes);
                    break;
            }
        }
        /// <summary>
        /// 广播消息
        /// </summary>
        /// <param name="msg">消息体</param>
        /// <param name="session">广播客户端</param>
        private static void BroadCast(IMessage msg, ClientSession session)
        {
            lock (_lock)
            {
                foreach(var sso in sessions)
                {
                    //判断是否为发送客户端
                    if (sso != session)
                    {
                        //sso.SendMsgAsync(MsgType.SyncPos, msg);
                    }
                }
            }
        }
    }
}