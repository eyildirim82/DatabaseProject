using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using DatabaseProject.Models;

namespace DatabaseProject.DAL
{
    public class AuthDAL
    {
        private readonly string _connectionString;

        public AuthDAL(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Şifre Hashleme Metodu (SHA256) - DB'deki formatla eşleşmeli
        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public (int UserId, string FullName, int RoleId)? LoginUser(string username, string password)
        {
            string passwordHash = ComputeSha256Hash(password); // Önce hashle

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_UserLogin", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    // Kullanıcı bulundu
                    return (
                        (int)reader["UserID"],
                        reader["FullName"].ToString(),
                        (int)reader["RoleID"]
                    );
                }
            }
            return null; // Kullanıcı bulunamadı
        }
    }
}