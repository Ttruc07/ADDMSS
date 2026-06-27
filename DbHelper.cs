using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace ADDMS2
{
    public static class DbHelper
    {
        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dbconfig.json");
        private static string _connectionString = "";

        static DbHelper()
        {
            LoadConnectionString();
        }

        public static string ConnectionString => _connectionString;

        public static void LoadConnectionString()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath);
                    using (JsonDocument doc = JsonDocument.Parse(json))
                    {
                        if (doc.RootElement.TryGetProperty("ConnectionString", out JsonElement prop))
                        {
                            _connectionString = prop.GetString() ?? "";
                            return;
                        }
                    }
                }
            }
            catch
            {
                // Bỏ qua nếu đọc thất bại
            }

            // Kết nối mặc định dự phòng
            _connectionString = "Server=.;Database=UAV_Management;Trusted_Connection=True;TrustServerCertificate=True";
        }

        public static void SaveConnectionString(string newConnStr)
        {
            _connectionString = newConnStr;
            try
            {
                var data = new { ConnectionString = newConnStr };
                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Không thể lưu file cấu hình: {ex.Message}");
            }
        }

        public static SqlConnection GetConnection(string? customConnStr = null)
        {
            return new SqlConnection(customConnStr ?? _connectionString);
        }

        public static bool TestConnection(string connStr, out string errorMsg)
        {
            errorMsg = "";
            try
            {
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                return false;
            }
        }

        public static DataTable ExecuteQuery(string sql, Dictionary<string, object>? parameters = null)
        {
            var dt = new DataTable();
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }

                using (var adapter = new SqlDataAdapter(cmd))
                {
                    conn.Open();
                    adapter.Fill(dt);
                }
            }
            return dt;
        }

        public static int ExecuteNonQuery(string sql, Dictionary<string, object>? parameters = null)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public static object? ExecuteScalar(string sql, Dictionary<string, object>? parameters = null)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }

                conn.Open();
                var result = cmd.ExecuteScalar();
                return result == DBNull.Value ? null : result;
            }
        }
    }
}
