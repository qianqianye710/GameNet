using MySqlConnector;
using UserCenter;

namespace UserCenterService.Tools
{
    internal class MySQL
    {
        private static string _connStr = 
            "server=localhost;" +
            "port=0501;" +
            "database=UserInfo_DB;" +
            "user=root;" +
            "password=ghy050501;" +
            "charset=utf8mb4";
        //private static MySqlConnection connection;

        //public static void InitDB()
        //{
        //    try
        //    {
        //        MySqlConnection connection = new MySqlConnection(_connStr);
        //        connection.Open();
        //        Console.WriteLine("数据库连接成功!");
        //        Console.WriteLine($"MySQL 版本：{connection.ServerVersion}");
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine($"数据库连接失败：{e.Message}");
        //        Environment.Exit(1);
        //    }
        //}
        public static void CreateTable(string tableName)
        {
            string sql = $@"
                CREATE TABLE IF NOT EXISTS {tableName}(
                    userID INT PRIMARY KEY AUTO_INCREMENT,
                    userName VARCHAR(50) NOT NULL,
                    password VARCHAR(100) NOT NULL,
                    createTime DATETIME DEFAULT NOW(),
                    lastLoginTime DATETIME null)
                    AUTO_INCREMENT = 1000011;";

            using MySqlConnection connection = new MySqlConnection(_connStr);
            connection.Open();
            using (var cmd = new MySqlCommand(sql, connection))
            {
                cmd.ExecuteNonQuery();
                Console.WriteLine($"{tableName}表创建完成");
            }
        }
        public static PlayerInfo QueryData(string tableName, string userName)
        {
            string sql = $@"
                SELECT userID, userName, password
                FROM {tableName}
                WHERE userName = @userName;";

            using MySqlConnection connection = new MySqlConnection(_connStr);
            connection.Open();
            using (var cmd = new MySqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@userName", userName);
                var result = cmd.ExecuteReader();
                if (result.Read())
                {
                    return new PlayerInfo
                    {
                        PlayerId = result.GetInt64("userID"),
                        Username = result.GetString("userName"),
                        Password = result.GetString("password")
                    };
                }
                return null;
            }
        }
        public static PlayerInfo QueryData(string tableName,long userID)
        {
            string sql = $@"
                SELECT userID, userName, password
                FROM {tableName}
                WHERE userID = @Primary;";

            using MySqlConnection connection = new MySqlConnection(_connStr);
            connection.Open();
            using (var cmd = new MySqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@Primary", userID);
                var result = cmd.ExecuteReader();
                if (result.Read())
                {
                    return new PlayerInfo
                    {
                        PlayerId = result.GetInt64("userID"),
                        Username = result.GetString("userName"),
                        Password = result.GetString("password")
                    };
                }
                return null;
            }
        }
        public static bool AddDataTOTable(string tableName ,string userName,string password,out long userID)
        {
            string sql = $@"
                INSERT INTO {tableName} (userName, password, createTime)
                VALUES (@userName, @password, NOW());";

            PlayerInfo info = QueryData(tableName, userName);
            if (info != null)
            {
                Console.WriteLine("账号已存在!");
                userID = info.PlayerId;

                return false;
            }

            using MySqlConnection connection = new MySqlConnection(_connStr);
            connection.Open();
            using (var cmd = new MySqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@userName", userName);
                cmd.Parameters.AddWithValue("@password", password);

                cmd.ExecuteNonQuery();
                userID = cmd.LastInsertedId;
                return true;
            }
        }
    }
}
