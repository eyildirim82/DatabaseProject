using Microsoft.Data.SqlClient;
using System.Data;
using DatabaseProject.Models;

namespace DatabaseProject.DAL
{
    public class CollectionNoteDAL
    {
        private readonly string _connectionString;

        public CollectionNoteDAL(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string not found.");
        }

        /// <summary>
        /// Müşteriye ait collection notlarını getirir
        /// </summary>
        public List<CollectionNote> GetCollectionNotes(int customerId)
        {
            var notes = new List<CollectionNote>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetCollectionNotes", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CustomerID", customerId);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            notes.Add(new CollectionNote
                            {
                                NoteID = reader.GetInt32(reader.GetOrdinal("NoteID")),
                                CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                                NoteText = reader.GetString(reader.GetOrdinal("NoteText")),
                                PromiseDate = reader.IsDBNull(reader.GetOrdinal("PromiseDate")) ? null : reader.GetDateTime(reader.GetOrdinal("PromiseDate")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                CompanyName = reader.GetString(reader.GetOrdinal("CompanyName")),
                                FullName = reader.GetString(reader.GetOrdinal("FullName"))
                            });
                        }
                    }
                }
            }

            return notes;
        }

        /// <summary>
        /// Kullanıcının yazdığı collection notlarını getirir
        /// </summary>
        public List<CollectionNote> GetCollectionNotesByUser(int userId)
        {
            var notes = new List<CollectionNote>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetCollectionNotesByUser", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            notes.Add(new CollectionNote
                            {
                                NoteID = reader.GetInt32(reader.GetOrdinal("NoteID")),
                                CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                                NoteText = reader.GetString(reader.GetOrdinal("NoteText")),
                                PromiseDate = reader.IsDBNull(reader.GetOrdinal("PromiseDate")) ? null : reader.GetDateTime(reader.GetOrdinal("PromiseDate")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                CompanyName = reader.GetString(reader.GetOrdinal("CompanyName")),
                                FullName = reader.GetString(reader.GetOrdinal("FullName"))
                            });
                        }
                    }
                }
            }

            return notes;
        }

        /// <summary>
        /// Tüm collection notlarını getirir (filtreleme ile)
        /// </summary>
        public List<CollectionNote> GetAllCollectionNotes(int? customerId = null)
        {
            var notes = new List<CollectionNote>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetAllCollectionNotes", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CustomerID", (object?)customerId ?? DBNull.Value);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            notes.Add(new CollectionNote
                            {
                                NoteID = reader.GetInt32(reader.GetOrdinal("NoteID")),
                                CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                                NoteText = reader.GetString(reader.GetOrdinal("NoteText")),
                                PromiseDate = reader.IsDBNull(reader.GetOrdinal("PromiseDate")) ? null : reader.GetDateTime(reader.GetOrdinal("PromiseDate")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                CompanyName = reader.GetString(reader.GetOrdinal("CompanyName")),
                                FullName = reader.GetString(reader.GetOrdinal("FullName"))
                            });
                        }
                    }
                }
            }

            return notes;
        }

        /// <summary>
        /// Collection note ekler (sp_AddCollectionNote stored procedure ile)
        /// </summary>
        public void AddCollectionNote(int customerId, int userId, string noteText, DateTime? promiseDate)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_AddCollectionNote", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@CustomerID", customerId);
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@NoteText", noteText);
                    cmd.Parameters.AddWithValue("@PromiseDate", (object?)promiseDate ?? DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Collection note ID'ye göre not bilgilerini getirir
        /// </summary>
        public CollectionNote? GetCollectionNoteById(int noteId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetCollectionNoteById", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@NoteID", noteId);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new CollectionNote
                            {
                                NoteID = reader.GetInt32(reader.GetOrdinal("NoteID")),
                                CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                                NoteText = reader.GetString(reader.GetOrdinal("NoteText")),
                                PromiseDate = reader.IsDBNull(reader.GetOrdinal("PromiseDate")) ? null : reader.GetDateTime(reader.GetOrdinal("PromiseDate")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                CompanyName = reader.GetString(reader.GetOrdinal("CompanyName")),
                                FullName = reader.GetString(reader.GetOrdinal("FullName"))
                            };
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Collection note günceller
        /// </summary>
        public void UpdateCollectionNote(int noteId, string noteText, DateTime? promiseDate)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_UpdateCollectionNote", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@NoteID", noteId);
                    cmd.Parameters.AddWithValue("@NoteText", noteText);
                    cmd.Parameters.AddWithValue("@PromiseDate", (object?)promiseDate ?? DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Collection note siler
        /// </summary>
        public void DeleteCollectionNote(int noteId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_DeleteCollectionNote", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@NoteID", noteId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
