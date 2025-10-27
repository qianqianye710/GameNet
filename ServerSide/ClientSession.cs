using GameNet;
using Google.Protobuf;
using System.Net.Sockets;

namespace ServerSide
{
    /// <summary>
    /// 单个客户端类
    /// </summary>
    internal class ClientSession
    {
        private string playerName = "Player";
        private TcpClient client;
        private NetworkStream netDataStream;
        private byte[] recvBuffer = new byte[4096];

        /// <summary>
        /// 客户端构造函数
        /// </summary>
        /// <param name="_client">连接对象</param>
        /// <param name="_msgHandler">消息处理委托</param>
        public ClientSession(TcpClient _client, Action<ClientSession, MsgType, byte[]> _msgHandler)
        {
            client = _client;
            netDataStream = client.GetStream();
            StartReceive(_msgHandler);
        }

        /// <summary>
        /// 启动异步接受客户端消息
        /// </summary>
        /// <param name="msgHandler"></param>
        private void StartReceive(Action<ClientSession, MsgType, byte[]> msgHandler)
        {
            //开始读取
            netDataStream.BeginRead(
                recvBuffer,
                0,
                recvBuffer.Length,
                (ar) =>
                {
                    try
                    {
                        //结束读取
                        int byteRead = netDataStream.EndRead(ar);
                        if (byteRead <= 0)
                        {
                            //断开连接
                            DisConnect();
                            return;
                        }
                        //解析消息
                        int offset = 0;//当前解析偏移量
                        while (offset + 3 <= byteRead)
                        {
                            //解析消息长度
                            ushort len = (ushort)((recvBuffer[offset] << 8) | recvBuffer[offset + 1]);
                            if (offset + 3 + len > byteRead) break;

                            //解析消息
                            MsgType msgType = (MsgType)recvBuffer[offset + 2];
                            byte[] data = new byte[len];
                            Array.Copy(recvBuffer, offset + 3, data, 0, len);

                            //调用消息处理委托
                            msgHandler(this, msgType, data);
                            offset += 3 + len;
                        }

                        //异步接收下一批数据
                        StartReceive(msgHandler);
                    }
                    catch
                    {
                        DisConnect();
                    }
                },
                null
            );
        }

        /// <summary>
        /// 发送消息方法
        /// </summary>
        /// <param name="msgType">消息类型</param>
        /// <param name="msg">序列化消息对象</param>
        public void SendMsg(MsgType msgType,IMessage msg)
        {
            try
            {
                //序列话消息对象
                byte[] data = msg.ToByteArray();
                ushort len = (ushort)data.Length;
                //构建消息包
                byte[] buffer = new byte[len + 3];
                buffer[0] = (byte)(len >> 8); // 长度高8位
                buffer[1] = (byte)(len & 0xFF); // 长度低8位 
                buffer[2] = (byte)msgType;
                Array.Copy(data, 0, buffer, 3, len);

                netDataStream.Write(buffer, 0, buffer.Length);
            }
            catch
            {
                DisConnect();
            }
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void DisConnect()
        {
            netDataStream?.Close();
            client?.Close();
            lock (Server._lock)
            {
                Server.sessions.Remove(this);
            }
            Console.WriteLine($"客户端:{playerName}断开连接");
        }
    }
}
