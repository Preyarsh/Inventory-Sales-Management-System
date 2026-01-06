using Inventory_Management_System_with_Sales_Management_API.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Inventory_Management_System_with_Sales_Management_API.Data
{
    public class CategoryRepository
    {
        private readonly string _connectionString;
        public CategoryRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public List<CategoryModel> GetAll()
        {
            List<CategoryModel> list = new();

            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("PR_Categories_SelectAll", con);
            cmd.CommandType = CommandType.StoredProcedure;

            con.Open();
            using SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new CategoryModel
                {
                    CategoryId = Convert.ToInt32(dr["CategoryId"]),
                    CategoryName = dr["CategoryName"].ToString(),
                    Description = dr["Description"].ToString(),
                    IsActive = Convert.ToBoolean(dr["IsActive"]),
                    CreatedAt = dr["CreatedAt"] != DBNull.Value
                        ? Convert.ToDateTime(dr["CreatedAt"])
                        : DateTime.MinValue
                });
            }
            return list;
        }
        public CategoryModel GetById(int id)
        {
            CategoryModel model = null;

            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("PR_Categories_SelectByID", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CategoryId", id);

            con.Open();
            using SqlDataReader dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                model = new CategoryModel
                {
                    CategoryId = Convert.ToInt32(dr["CategoryId"]),
                    CategoryName = dr["CategoryName"].ToString(),
                    Description = dr["Description"].ToString(),
                    IsActive = Convert.ToBoolean(dr["IsActive"]),
                    CreatedAt = dr["CreatedAt"] != DBNull.Value
                        ? Convert.ToDateTime(dr["CreatedAt"])
                        : DateTime.MinValue
                };
            }
            return model;
        }
        public void Insert(CategoryModel model)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("PR_Categories_Insert", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@CategoryName", model.CategoryName);
            cmd.Parameters.AddWithValue("@Description", model.Description ?? "");

            con.Open();
            cmd.ExecuteNonQuery();
        }
        public void Update(CategoryModel model)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("PR_Categories_Update", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@CategoryId", model.CategoryId);
            cmd.Parameters.AddWithValue("@CategoryName", model.CategoryName);
            cmd.Parameters.AddWithValue("@Description", model.Description ?? "");
            cmd.Parameters.AddWithValue("@IsActive", model.IsActive);

            con.Open();
            cmd.ExecuteNonQuery();
        }
        public void Delete(int id)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("PR_Categories_Delete", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CategoryId", id);

            con.Open();
            cmd.ExecuteNonQuery();
        }
        public List<CategoryModel> GetForDropdown()
        {
            List<CategoryModel> list = new();

            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("PR_Categories_SelectForDropdown", con);
            cmd.CommandType = CommandType.StoredProcedure;

            con.Open();
            using SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new CategoryModel
                {
                    CategoryId = Convert.ToInt32(dr["CategoryId"]),
                    CategoryName = dr["CategoryName"].ToString()
                });
            }
            return list;
        }

    }
}
