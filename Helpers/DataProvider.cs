using System.Data;
using Microsoft.Data.SqlClient;

namespace CafeManager.Helpers;

public static class DataProvider
{
    // Đọc từ App.config (mục connectionStrings -> CafeDB)
    public static string ConnStr =>
        System.Configuration.ConfigurationManager.ConnectionStrings["CafeDB"].ConnectionString;

    static void AddParams(SqlCommand cmd, (string, object)[] ps)
    {
        foreach (var (name, value) in ps)
            cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
    }

    // ---- Truy vấn thường (tự mở/đóng kết nối) ----
    public static DataTable Query(string sql, params (string, object)[] ps)
    {
        using var conn = new SqlConnection(ConnStr);
        using var cmd = new SqlCommand(sql, conn);
        AddParams(cmd, ps);
        var dt = new DataTable();
        new SqlDataAdapter(cmd).Fill(dt);
        return dt;
    }

    public static object Scalar(string sql, params (string, object)[] ps)
    {
        using var conn = new SqlConnection(ConnStr);
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        AddParams(cmd, ps);
        var o = cmd.ExecuteScalar();
        return o == DBNull.Value ? null : o;
    }

    public static int Exec(string sql, params (string, object)[] ps)
    {
        using var conn = new SqlConnection(ConnStr);
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        AddParams(cmd, ps);
        return cmd.ExecuteNonQuery();
    }

    // ---- Dùng bên trong transaction ----
    public static object Scalar(SqlConnection c, SqlTransaction t, string sql, params (string, object)[] ps)
    {
        using var cmd = new SqlCommand(sql, c, t);
        AddParams(cmd, ps);
        var o = cmd.ExecuteScalar();
        return o == DBNull.Value ? null : o;
    }

    public static int Exec(SqlConnection c, SqlTransaction t, string sql, params (string, object)[] ps)
    {
        using var cmd = new SqlCommand(sql, c, t);
        AddParams(cmd, ps);
        return cmd.ExecuteNonQuery();
    }

    public static DataTable Query(SqlConnection c, SqlTransaction t, string sql, params (string, object)[] ps)
    {
        using var cmd = new SqlCommand(sql, c, t);
        AddParams(cmd, ps);
        var dt = new DataTable();
        new SqlDataAdapter(cmd).Fill(dt);
        return dt;
    }
}
