using Inventory_Management_System_with_Sales_Management_API.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Inventory_Management_System_with_Sales_Management_API.Data
{
    public class UnitRepository
    {
        private readonly string _connectionString;

        public UnitRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // ✅ Get All Units (Search + Status Filter)
        public List<UnitModel> GetAll(string? search, bool? status)
        {
            List<UnitModel> list = new();

            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("PR_Units_SelectAll", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Search", search ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", status ?? (object)DBNull.Value);

            con.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                list.Add(new UnitModel
                {
                    UnitID = Convert.ToInt32(dr["UnitID"]),
                    UnitName = dr["UnitName"].ToString(),
                    UnitShortName = dr["UnitShortName"].ToString(),
                    Description = dr["Description"]?.ToString(),
                    IsActive = Convert.ToBoolean(dr["IsActive"])
                });
            }

            return list;
        }

        // ✅ Get Unit By ID
        public UnitModel GetById(int id)
        {
            UnitModel unit = null;

            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("PR_Units_SelectByID", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@UnitID", id);

            con.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                unit = new UnitModel
                {
                    UnitID = Convert.ToInt32(dr["UnitID"]),
                    UnitName = dr["UnitName"].ToString(),
                    UnitShortName = dr["UnitShortName"].ToString(),
                    Description = dr["Description"]?.ToString(),
                    IsActive = Convert.ToBoolean(dr["IsActive"])
                };
            }

            return unit;
        }

        // ✅ Insert Unit
        public void Insert(UnitModel model)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("PR_Units_Insert", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@UnitName", model.UnitName);
            cmd.Parameters.AddWithValue("@UnitShortName", model.UnitShortName);
            cmd.Parameters.AddWithValue("@Description", model.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", model.IsActive);

            con.Open();
            cmd.ExecuteNonQuery();
        }

        // ✅ Update Unit
        public void Update(UnitModel model)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("PR_Units_Update", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@UnitID", model.UnitID);
            cmd.Parameters.AddWithValue("@UnitName", model.UnitName);
            cmd.Parameters.AddWithValue("@UnitShortName", model.UnitShortName);
            cmd.Parameters.AddWithValue("@Description", model.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", model.IsActive);

            con.Open();
            cmd.ExecuteNonQuery();
        }

        // ✅ Delete Unit
        public void Delete(int id)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("PR_Units_Delete", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@UnitID", id);

            con.Open();
            cmd.ExecuteNonQuery();
        }
        public List<UnitModel> GetForDropdown()
        {
            List<UnitModel> list = new();

            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("PR_Units_SelectForDropdown", con);
            cmd.CommandType = CommandType.StoredProcedure;

            con.Open();
            using SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new UnitModel
                {
                    UnitID = Convert.ToInt32(dr["UnitId"]),
                    UnitName = dr["UnitName"].ToString()
                });
            }
            return list;
        }

    }
}
