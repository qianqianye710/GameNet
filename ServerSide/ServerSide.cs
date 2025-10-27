using System.Net;
using System.Net.Sockets;
using Google.Protobuf;
using GameNet;
using MySqlConnector;
using BCrypt;

namespace ServerSide
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
        private static void HandleMsg(ClientSession session,MsgType msgType, byte[] data)
        {
            switch (msgType)
            {
                case MsgType.LoginReq:
                    //反序列化消息为LoginRequest对象
                    var loginReq = LoginRequest.Parser.ParseFrom(data);
                    //创建登录响应对象
                    var loginRes = new LoginResponse
                    {
                        Success = CheckLoginInfo(loginReq)
                    };
                    loginRes.Msg = loginRes.Success ? "登陆成功" : "账号密码错误";
                    //给客户端发送登录结果消息
                    session.SendMsg(MsgType.LoginRsp, loginRes);
                    break;
                //case MsgType.SyncPos:
                    //var syvcPos = SyncPosition.Parser.ParseFrom(data);
                    //广播位置消息
                    //BroadCast(syvcPos, session);
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
                        //sso.SendMsg(MsgType.SyncPos, msg);
                    }
                }
            }
        }

        private static bool CheckLoginInfo(LoginRequest loginReq)
        {
            if (loginReq.Username == "test" && loginReq.Password == "1220")
                return true;
            else if (loginReq.Username == "gu" && loginReq.Password == "1026")
                return true;
            return false;
        }
    }
}