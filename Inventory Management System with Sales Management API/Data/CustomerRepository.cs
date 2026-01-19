using Inventory_Management_System_with_Sales_Management_API.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Inventory_Management_System_with_Sales_Management_API.Data
{
    public class CustomerRepository
    {
        private readonly string _connectionString;

        public CustomerRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // ✅ GET ALL CUSTOMERS
        public List<CustomerModel> GetAll()
        {
            List<CustomerModel> list = new();

            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("PR_Customer_GetAll", con);
            cmd.CommandType = CommandType.StoredProcedure;

            con.Open();
            using SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new CustomerModel
                {
                    CustomerId = Convert.ToInt32(dr["CustomerId"]),
                    CustomerName = dr["CustomerName"].ToString(),
                    CustomerMobile = dr["CustomerMobile"].ToString(),
                    Email = dr["Email"].ToString(),
                    BillingAddress = dr["BillingAddress"].ToString(),
                    ShippingAddress = dr["ShippingAddress"].ToString(),
                    GSTNumber = dr["GSTNumber"].ToString(),
                    IsActive = Convert.ToBoolean(dr["IsActive"]),
                    CreatedBy = Convert.ToInt32(dr["CreatedBy"]),
                    CreatedAt = Convert.ToDateTime(dr["CreatedAt"]),
                    UpdatedBy = dr["UpdatedBy"] == DBNull.Value ? null : Convert.ToInt32(dr["UpdatedBy"]),
                    UpdatedAt = dr["UpdatedAt"] == DBNull.Value ? null : Convert.ToDateTime(dr["UpdatedAt"])
                });
            }

            return list;
        }

        // ✅ GET CUSTOMER BY ID
        public CustomerModel GetById(int id)
        {
            CustomerModel model = null;

            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("PR_Customer_GetById", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CustomerId", id);

            con.Open();
            using SqlDataReader dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                model = new CustomerModel
                {
                    CustomerId = Convert.ToInt32(dr["CustomerId"]),
                    CustomerName = dr["CustomerName"].ToString(),
                    CustomerMobile = dr["CustomerMobile"].ToString(),
                    Email = dr["Email"].ToString(),
                    BillingAddress = dr["BillingAddress"].ToString(),
                    ShippingAddress = dr["ShippingAddress"].ToString(),
                    GSTNumber = dr["GSTNumber"].ToString(),
                    IsActive = Convert.ToBoolean(dr["IsActive"]),
                    CreatedBy = Convert.ToInt32(dr["CreatedBy"]),
                    CreatedAt = Convert.ToDateTime(dr["CreatedAt"]),
                    UpdatedBy = dr["UpdatedBy"] == DBNull.Value ? null : Convert.ToInt32(dr["UpdatedBy"]),
                    UpdatedAt = dr["UpdatedAt"] == DBNull.Value ? null : Convert.ToDateTime(dr["UpdatedAt"])
                };
            }

            return model;
        }

        // ✅ INSERT CUSTOMER
        public bool Insert(CustomerModel model)
        {
            try
            {
                using SqlConnection con = new SqlConnection(_connectionString);
                using SqlCommand cmd = new SqlCommand("PR_Customer_Insert", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@CustomerName", model.CustomerName);
                cmd.Parameters.AddWithValue("@CustomerMobile", model.CustomerMobile);
                cmd.Parameters.AddWithValue("@Email", model.Email);
                cmd.Parameters.AddWithValue("@BillingAddress", model.BillingAddress);
                cmd.Parameters.AddWithValue("@ShippingAddress", model.ShippingAddress);
                cmd.Parameters.AddWithValue("@GSTNumber", model.GSTNumber);
                cmd.Parameters.AddWithValue("@CreatedBy", model.CreatedBy);

                con.Open();
                cmd.ExecuteNonQuery();

                return true;
            }
            catch
            {
                return false;
            }
        }

        // ✅ UPDATE CUSTOMER
        public bool Update(CustomerModel model)
        {
            try
            {
                using SqlConnection con = new SqlConnection(_connectionString);
                using SqlCommand cmd = new SqlCommand("PR_Customer_Update", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@CustomerId", model.CustomerId);
                cmd.Parameters.AddWithValue("@CustomerName", model.CustomerName);
                cmd.Parameters.AddWithValue("@CustomerMobile", model.CustomerMobile);
                cmd.Parameters.AddWithValue("@Email", model.Email);
                cmd.Parameters.AddWithValue("@BillingAddress", model.BillingAddress);
                cmd.Parameters.AddWithValue("@ShippingAddress", model.ShippingAddress);
                cmd.Parameters.AddWithValue("@GSTNumber", model.GSTNumber);
                cmd.Parameters.AddWithValue("@UpdatedBy", model.UpdatedBy);

                con.Open();
                cmd.ExecuteNonQuery();

                return true;
            }
            catch
            {
                return false;
            }
        }

        // ✅ DELETE CUSTOMER (SOFT DELETE)
        public bool Delete(int id, int deletedBy)
        {
            try
            {
                using SqlConnection con = new SqlConnection(_connectionString);
                using SqlCommand cmd = new SqlCommand("PR_Customer_Delete", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@CustomerId", id);
                cmd.Parameters.AddWithValue("@DeletedBy", deletedBy);

                con.Open();
                cmd.ExecuteNonQuery();

                return true;
            }
            catch
            {
                return false;
            }
        }

        // ✅ CUSTOMER DROPDOWN (for Sales Order / Invoice)
        //public List<CustomerLookupModel> GetCustomerDropdown()
        //{
        //    var list = new List<CustomerLookupModel>();

        //    using SqlConnection con = new SqlConnection(_connectionString);
        //    using SqlCommand cmd = new SqlCommand("sp_Customer_Dropdown", con);
        //    cmd.CommandType = CommandType.StoredProcedure;

        //    con.Open();
        //    using SqlDataReader dr = cmd.ExecuteReader();
        //    while (dr.Read())
        //    {
        //        list.Add(new CustomerLookupModel
        //        {
        //            CustomerId = Convert.ToInt32(dr["CustomerId"]),
        //            CustomerName = dr["CustomerName"].ToString()
        //        });
        //    }

        //    return list;
        //}
    }
}
