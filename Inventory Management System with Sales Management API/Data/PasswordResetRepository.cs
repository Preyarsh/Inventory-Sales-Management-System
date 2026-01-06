using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Inventory_Management_System_with_Sales_Management_API.Data
{
    public class PasswordResetRepository : IPasswordResetRepository
    {
        private readonly string _connectionString;

        public PasswordResetRepository(IConfiguration configuration)
        {
            if (configuration is null) throw new ArgumentNullException(nameof(configuration));

            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        // ---------------------------------------------------------
        // ⭐ STEP 1 – Insert into PasswordResetOTP Table
        // ---------------------------------------------------------
        public bool InsertPasswordResetOTP(string email, string otp, DateTime expiryTime)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("PR_ForgotPassword_SaveOTP", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@OTP", otp);
                cmd.Parameters.AddWithValue("@ExpiryTime", expiryTime);

                connection.Open();
                // We check if the INSERT worked (rows affected > 0)
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ---------------------------------------------------------
        // ⭐ STEP 2 – Verify from PasswordResetOTP Table
        // ---------------------------------------------------------
        public bool VerifyOTP(string email, string otp)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("PR_ForgotPassword_ValidateOTP", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@OTP", otp);

                connection.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    return reader.HasRows;
                }
            }
        }

        // ---------------------------------------------------------
        // ⭐ STEP 3 – Update Password (Users) & Mark OTP Used (PasswordResetOTP)
        // ---------------------------------------------------------
        public bool UpdatePassword(string email, string newPassword)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("PR_ForgotPassword_Reset", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Password", newPassword);

                connection.Open();

                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

        // ---------------------------------------------------------
        // ⭐ Compatibility method required by IPasswordResetRepository
        // ---------------------------------------------------------
        // Implements interface member ResetPasswordByUserId(int, string)
        public bool ResetPasswordByUserId(int userId, string newPassword)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("PR_PasswordResetOTP_Reset_ByUserId", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Password", newPassword);

                connection.Open();

                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }
    }
}