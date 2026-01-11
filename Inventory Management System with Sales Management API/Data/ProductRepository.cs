using Inventory_Management_System_with_Sales_Management_API.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Inventory_Management_System_with_Sales_Management_API.Data
{
    public class ProductRepository
    {
        private readonly string _connectionString;

        public ProductRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // ✅ GET ALL PRODUCTS (Category + Unit Name)
        public List<ProductModel> GetAll()
        {
            List<ProductModel> list = new();

            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("PR_Products_SelectAll", con);
            cmd.CommandType = CommandType.StoredProcedure;

            con.Open();
            using SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new ProductModel
                {
                    ProductId = Convert.ToInt32(dr["ProductId"]),
                    ProductName = dr["ProductName"].ToString(),
                    ProductCode = dr["ProductCode"].ToString(),
                    //CategoryId = Convert.ToInt32(dr["CategoryId"]),
                    CategoryName = dr["CategoryName"].ToString(),
                    //UnitId = Convert.ToInt32(dr["UnitId"]),
                    UnitName = dr["UnitName"].ToString(),
                    PurchasePrice = Convert.ToDecimal(dr["PurchasePrice"]),
                    SalePrice = Convert.ToDecimal(dr["SalePrice"]),
                    //OpeningStock = Convert.ToInt32(dr["OpeningStock"]),
                    CurrentStock = Convert.ToInt32(dr["CurrentStock"]),
                    IsActive = Convert.ToBoolean(dr["IsActive"])
                });
            }

            return list;
        }

        // ✅ GET PRODUCT BY ID
        public ProductModel GetById(int id)
        {
            ProductModel model = null;

            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("PR_Products_SelectById", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ProductId", id);

            con.Open();
            using SqlDataReader dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                model = new ProductModel
                {
                    ProductId = Convert.ToInt32(dr["ProductId"]),
                    ProductName = dr["ProductName"].ToString(),
                    ProductCode = dr["ProductCode"].ToString(),
                    CategoryId = Convert.ToInt32(dr["CategoryId"]),
                    UnitId = Convert.ToInt32(dr["UnitId"]),
                    PurchasePrice = Convert.ToDecimal(dr["PurchasePrice"]),
                    SalePrice = Convert.ToDecimal(dr["SalePrice"]),
                    OpeningStock = Convert.ToInt32(dr["OpeningStock"]),
                    CurrentStock = Convert.ToInt32(dr["CurrentStock"]),
                    IsActive = Convert.ToBoolean(dr["IsActive"])
                };
            }

            return model;
        }

        // ✅ INSERT PRODUCT
        public bool Insert(ProductModel model)
        {
            try
            {
                using SqlConnection con = new SqlConnection(_connectionString);
                using SqlCommand cmd = new SqlCommand("PR_Products_Insert", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@ProductName", model.ProductName);
                cmd.Parameters.AddWithValue("@ProductCode", model.ProductCode);
                cmd.Parameters.AddWithValue("@CategoryId", model.CategoryId);
                cmd.Parameters.AddWithValue("@UnitId", model.UnitId);
                cmd.Parameters.AddWithValue("@PurchasePrice", model.PurchasePrice);
                cmd.Parameters.AddWithValue("@SalePrice", model.SalePrice);
                cmd.Parameters.AddWithValue("@OpeningStock", model.OpeningStock);
                cmd.Parameters.AddWithValue("@CurrentStock", model.CurrentStock);
                cmd.Parameters.AddWithValue("@IsActive", model.IsActive);

                con.Open();
                cmd.ExecuteNonQuery();

                return true;
            }
            catch
            {
                return false;
            }
        }


        // ✅ UPDATE PRODUCT
        public bool Update(ProductModel model)
        {
            try
            {
                using SqlConnection con = new SqlConnection(_connectionString);
                using SqlCommand cmd = new SqlCommand("PR_Products_Update", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@ProductId", model.ProductId);
                cmd.Parameters.AddWithValue("@ProductName", model.ProductName);
                cmd.Parameters.AddWithValue("@ProductCode", model.ProductCode);
                cmd.Parameters.AddWithValue("@CategoryId", model.CategoryId);
                cmd.Parameters.AddWithValue("@UnitId", model.UnitId);
                cmd.Parameters.AddWithValue("@PurchasePrice", model.PurchasePrice);
                cmd.Parameters.AddWithValue("@SalePrice", model.SalePrice);
                cmd.Parameters.AddWithValue("@OpeningStock", model.OpeningStock);
                cmd.Parameters.AddWithValue("@CurrentStock", model.CurrentStock);
                cmd.Parameters.AddWithValue("@IsActive", model.IsActive);

                con.Open();
                cmd.ExecuteNonQuery();

                return true;
            }
            catch
            {
                return false;
            }
        }


        // ✅ DELETE PRODUCT (SOFT DELETE)
        public bool Delete(int id)
        {
            try
            {
                using SqlConnection con = new SqlConnection(_connectionString);
                using SqlCommand cmd = new SqlCommand("PR_Products_Delete", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ProductId", id);

                con.Open();
                cmd.ExecuteNonQuery();

                return true;
            }
            catch
            {
                return false;
            }
        }
        public List<ProductLookupModel> GetProductsForSales()
        {
            var list = new List<ProductLookupModel>();

            using SqlConnection con = new SqlConnection(_connectionString);

            using SqlCommand cmd = new SqlCommand("PR_Product_Lookup_ForSales", con);
            cmd.CommandType = CommandType.StoredProcedure;

            con.Open();
            using SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                list.Add(new ProductLookupModel
                {
                    ProductId = Convert.ToInt32(dr["ProductId"]),
                    ProductName = dr["ProductName"].ToString(),
                    SalePrice = Convert.ToDecimal(dr["SalePrice"]),
                    AvailableStock = Convert.ToInt32(dr["AvailableStock"])
                });
            }

            return list;
        }

    }
}
