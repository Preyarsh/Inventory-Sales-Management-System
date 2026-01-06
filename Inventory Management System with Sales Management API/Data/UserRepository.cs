using Inventory_Management_System_with_Sales_Management_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
namespace Inventory_Management_System_with_Sales_Management_API.Data
{
    public class UserRepository
    {
        private readonly string _connectionString;
        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public IEnumerable<UserModel> GetAll()
        {
            var users = new List<UserModel>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("PR_Users_SelectAll", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                connection.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new UserModel
                        {
                            UserID = reader["UserID"] != DBNull.Value
                                        ? Convert.ToInt32(reader["UserID"]) : 0,

                            FullName = reader["FullName"]?.ToString(),

                            Email = reader["Email"]?.ToString(),

                            PasswordHash = reader["PasswordHash"]?.ToString(),

                            CreatedAt = reader["CreatedAt"] != DBNull.Value
                                        ? Convert.ToDateTime(reader["CreatedAt"])
                                        : DateTime.MinValue,

                            IsAdmin = reader["IsAdmin"] != DBNull.Value
                                        && Convert.ToBoolean(reader["IsAdmin"]),

                            IsActive = reader["IsActive"] != DBNull.Value
                                        && Convert.ToBoolean(reader["IsActive"])
                        });
                    }
                }
            }

            return users;
        }
        public UserModel GetByID(int userId)
        {
            UserModel user = null;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("PR_Users_SelectByPK", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@UserID", userId);

                connection.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new UserModel
                        {
                            UserID = Convert.ToInt32(reader["UserID"]),
                            FullName = reader["FullName"].ToString(),
                            Email = reader["Email"].ToString(),
                            PasswordHash = reader["PasswordHash"].ToString(),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                            IsAdmin = Convert.ToBoolean(reader["IsAdmin"]),
                            IsActive = Convert.ToBoolean(reader["IsActive"]),
                            DOB = reader["DOB"] == DBNull.Value ? null : Convert.ToDateTime(reader["DOB"]),
                            Phone = reader["Phone"] == DBNull.Value ? null : reader["Phone"].ToString(),
                            Address = reader["Address"] == DBNull.Value ? null : reader["Address"].ToString()
                        };
                    }
                }
            }

            return user;
        }
        public int Insert(UserModel model)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("PR_Users_Insert", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@FullName", model.FullName);
                cmd.Parameters.AddWithValue("@Email", model.Email);
                cmd.Parameters.AddWithValue("@PasswordHash", model.PasswordHash);
                cmd.Parameters.AddWithValue("@IsAdmin", model.IsAdmin);
                cmd.Parameters.AddWithValue("@IsActive", model.IsActive);

                connection.Open();
                return cmd.ExecuteNonQuery();
            }
        }
        public int Update(UpdateUserModel model)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            using SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                // 1️⃣ Update user details
                using (SqlCommand cmd = new SqlCommand("PR_Users_Update", connection, transaction))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@UserID", model.UserID);
                    cmd.Parameters.AddWithValue("@FullName", model.FullName);
                    cmd.Parameters.AddWithValue("@DOB", (object?)model.DOB ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Phone", model.Phone ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address", model.Address ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsActive", model.IsActive);
                    cmd.Parameters.AddWithValue("@IsAdmin", model.IsAdmin);

                    cmd.ExecuteNonQuery();
                }

                // 2️⃣ Update password ONLY if hash exists
                if (!string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_UpdateUserPasswordById", connection, transaction))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@UserID", model.UserID);
                        cmd.Parameters.AddWithValue("@PasswordHash", model.NewPassword);

                        cmd.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
                return 1;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }



        public int Delete(int userId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("PR_Users_Delete", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@UserID", userId);

                connection.Open();
                return cmd.ExecuteNonQuery();
            }
        }
        public UserModel Login(string email, string password)
        {
            UserModel user = null;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("PR_LoginUser", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 30
                };

                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@PasswordHash", password);

                connection.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        bool HasColumn(string name)
                        {
                            try { return reader.GetOrdinal(name) >= 0; }
                            catch (IndexOutOfRangeException) { return false; }
                        }
                        user = new UserModel
                        {
                            UserID = HasColumn("UserID") ? Convert.ToInt32(reader["UserID"]) : 0,
                            FullName = HasColumn("FullName") ? reader["FullName"]?.ToString() : null,
                            Email = HasColumn("Email") ? reader["Email"]?.ToString() : null,
                            PasswordHash = HasColumn("PasswordHash") ? reader["PasswordHash"]?.ToString() : null,
                            CreatedAt = HasColumn("CreatedAt") && reader["CreatedAt"] != DBNull.Value
                                        ? Convert.ToDateTime(reader["CreatedAt"])
                                        : DateTime.MinValue,
                            IsAdmin = HasColumn("IsAdmin") && reader["IsAdmin"] != DBNull.Value
                                      && Convert.ToBoolean(reader["IsAdmin"]),
                            IsActive = HasColumn("IsActive") && reader["IsActive"] != DBNull.Value
                                       && Convert.ToBoolean(reader["IsActive"])
                        };
                    }
                }
            }

            return user;
        }
        public UserModel GetByEmail(string email)
        {
            UserModel user = null;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("PR_Users_GetByEmail", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 50
                };

                cmd.Parameters.AddWithValue("@Email", email);

                connection.Open();
                

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new UserModel
                        {
                            UserID = Convert.ToInt32(reader["UserID"]),
                            FullName = reader["FullName"].ToString(),
                            Email = reader["Email"].ToString(),
                            PasswordHash = reader["PasswordHash"].ToString(),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                            IsAdmin = Convert.ToBoolean(reader["IsAdmin"]),
                            IsActive = Convert.ToBoolean(reader["IsActive"])
                        };
                    }
                }
            }

            return user;
        }

        public void UpdateUserPassword(int userId, string passwordHash)
        {
            using SqlConnection con = new SqlConnection(_connectionString);

            SqlCommand cmd = new SqlCommand("PR_UpdateUserPasswordById", con)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);

            con.Open();
            cmd.ExecuteNonQuery();
        }
        

    }
}
