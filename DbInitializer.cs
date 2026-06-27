using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace ADDMS2
{
    public static class DbInitializer
    {
        private static readonly string SchemaPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "UAV_Managementt.sql");
        private static readonly string DummyDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "dummydatatest.sql");

        public static void InitializeDatabase(string serverName, string authMode, string username, string password, Action<string> logCallback)
        {
            // Xây dựng connection string kết nối tới master
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = "master",
                TrustServerCertificate = true
            };

            if (authMode == "Windows")
            {
                builder.IntegratedSecurity = true;
            }
            else
            {
                builder.IntegratedSecurity = false;
                builder.UserID = username;
                builder.Password = password;
            }

            string masterConnStr = builder.ConnectionString;

            // 1. Tạo database nếu chưa tồn tại
            logCallback("Đang kiểm tra và khởi tạo database UAV_Management...");
            string createDbSql = @"
                IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'UAV_Management')
                BEGIN
                    CREATE DATABASE [UAV_Management];
                END";

            using (var conn = new SqlConnection(masterConnStr))
            using (var cmd = new SqlCommand(createDbSql, conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
                logCallback("Khởi tạo database UAV_Management thành công.");
            }

            // Xây dựng connection string trỏ vào database UAV_Management
            builder.InitialCatalog = "UAV_Management";
            string targetConnStr = builder.ConnectionString;

            // 2. Chạy script tạo bảng
            if (File.Exists(SchemaPath))
            {
                logCallback("Đang chạy script tạo cấu trúc bảng (UAV_Managementt.sql)...");
                ExecuteSqlScript(targetConnStr, SchemaPath, logCallback);
                logCallback("Tạo cấu trúc bảng thành công.");
            }
            else
            {
                throw new FileNotFoundException("Không tìm thấy file script UAV_Managementt.sql");
            }

            // 3. Chạy script chèn dữ liệu mẫu
            if (File.Exists(DummyDataPath))
            {
                logCallback("Đang chạy script chèn dữ liệu mẫu (dummydatatest.sql)...");
                ExecuteSqlScript(targetConnStr, DummyDataPath, logCallback);
                logCallback("Nhập dữ liệu mẫu thành công.");
            }
            else
            {
                throw new FileNotFoundException("Không tìm thấy file script dummydatatest.sql");
            }

            // Lưu connection string vào cấu hình
            DbHelper.SaveConnectionString(targetConnStr);
            logCallback("Đã cấu hình Connection String mới vào dbconfig.json.");
        }

        private static void ExecuteSqlScript(string connectionString, string scriptFilePath, Action<string> logCallback)
        {
            string scriptText = File.ReadAllText(scriptFilePath);

            // Tách các lệnh con theo từ khóa GO đứng riêng lẻ trên một dòng
            string[] commands = Regex.Split(
                scriptText,
                @"^\s*GO\s*$",
                RegexOptions.Multiline | RegexOptions.IgnoreCase);

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                foreach (string cmdText in commands)
                {
                    string trimmedCmd = cmdText.Trim();
                    if (string.IsNullOrEmpty(trimmedCmd)) continue;

                    using (var cmd = new SqlCommand(trimmedCmd, conn))
                    {
                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            // Bỏ qua lỗi nếu đối tượng đã tồn tại (cho phép chạy lại script nhiều lần)
                            if (!ex.Message.Contains("already exists") && 
                                !ex.Message.Contains("Already exists") &&
                                !ex.Message.Contains("there is already an object"))
                            {
                                logCallback($"[Lỗi thực thi lệnh]: {ex.Message}");
                                throw;
                            }
                        }
                    }
                }
            }
        }
    }
}
