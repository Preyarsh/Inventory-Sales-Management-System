using Inventory_Management_System_with_Sales_Management_API.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Inventory_Management_System_with_Sales_Management_API.Data
{
    public class ProductionRepository
    {
        private readonly string _connectionString;

        public ProductionRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }


        public int Insert(ProductionModel model)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("PR_Productions_Insert", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@ProductId", model.ProductId);
            cmd.Parameters.AddWithValue("@QuantityProduced", model.QuantityProduced);
            cmd.Parameters.AddWithValue("@ProductionDate", model.ProductionDate);
            cmd.Parameters.AddWithValue("@BatchNo", model.BatchNo ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Notes", model.Notes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedBy", model.CreatedBy);

            con.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }


        public void Update(int productionId, ProductionModel model, int updatedBy)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("PR_Productions_Update", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@ProductionId", productionId);
            cmd.Parameters.AddWithValue("@ProductId", model.ProductId);
            cmd.Parameters.AddWithValue("@QuantityProduced", model.QuantityProduced);
            cmd.Parameters.AddWithValue("@ProductionDate", model.ProductionDate);
            cmd.Parameters.AddWithValue("@BatchNo", model.BatchNo ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Notes", model.Notes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);

            con.Open();
            cmd.ExecuteNonQuery();
        }

        public void Delete(int productionId, int deletedBy)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("PR_Productions_Delete", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@ProductionId", productionId);
            cmd.Parameters.AddWithValue("@DeletedBy", deletedBy);

            con.Open();
            cmd.ExecuteNonQuery();
        }

        public List<ProductionListDto> GetAll()
        {
            List<ProductionListDto> list = new();

            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("PR_Productions_SelectAll", con);
            cmd.CommandType = CommandType.StoredProcedure;

            con.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                list.Add(new ProductionListDto
                {
                    ProductionId = Convert.ToInt32(dr["ProductionId"]),
                    ProductName = dr["ProductName"].ToString()!,
                    QuantityProduced = Convert.ToInt32(dr["QuantityProduced"]),
                    ProductionDate = Convert.ToDateTime(dr["ProductionDate"]),
                    BatchNo = dr["BatchNo"]?.ToString(),
                    Notes = dr["Notes"]?.ToString(),
                    CreatedByName = dr["CreatedByName"].ToString()!
                });
            }

            return list;
        }

        public ProductionModel? GetById(int id)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("PR_Productions_SelectById", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ProductionId", id);

            con.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            if (!dr.Read()) return null;

            return new ProductionModel
            {
                ProductionId = id,
                ProductId = Convert.ToInt32(dr["ProductId"]),
                QuantityProduced = Convert.ToInt32(dr["QuantityProduced"]),
                ProductionDate = Convert.ToDateTime(dr["ProductionDate"]),
                BatchNo = dr["BatchNo"]?.ToString(),
                Notes = dr["Notes"]?.ToString()
            };
        }
    }
}
