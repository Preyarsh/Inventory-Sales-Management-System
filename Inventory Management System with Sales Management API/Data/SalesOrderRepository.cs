using Inventory_Management_System_with_Sales_Management_API.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Inventory_Management_System_with_Sales_Management_API.Data
{
    public class SalesOrderRepository : ISalesOrderRepository
    {
        private readonly IConfiguration _configuration;

        public SalesOrderRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection()
            => new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        public int CreateSalesOrder(SalesOrderModel model)
        {
            using var con = GetConnection();
            con.Open();
            using var tran = con.BeginTransaction();

            try
            {
                // Generate Order No
                var orderNoCmd = new SqlCommand("PR_SalesOrder_GenerateOrderNo", con, tran);
                orderNoCmd.CommandType = CommandType.StoredProcedure;
                model.OrderNo = orderNoCmd.ExecuteScalar().ToString();

                // Insert Order
                var cmd = new SqlCommand("PR_SalesOrder_Insert", con, tran);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@OrderNo", model.OrderNo);
                cmd.Parameters.AddWithValue("@OrderDate", model.OrderDate);
                cmd.Parameters.AddWithValue("@CustomerName", model.CustomerName);
                cmd.Parameters.AddWithValue("@CustomerMobile", model.CustomerMobile);
                cmd.Parameters.AddWithValue("@SubTotal", model.SubTotal);
                cmd.Parameters.AddWithValue("@Discount", model.Discount);
                cmd.Parameters.AddWithValue("@Tax", model.Tax);
                cmd.Parameters.AddWithValue("@GrandTotal", model.GrandTotal);
                cmd.Parameters.AddWithValue("@CreatedBy", model.CreatedBy);

                int salesOrderId = Convert.ToInt32(cmd.ExecuteScalar());

                // Insert Items
                foreach (var item in model.Items)
                {
                    var itemCmd = new SqlCommand("PR_SalesOrderItems_Insert", con, tran);
                    itemCmd.CommandType = CommandType.StoredProcedure;
                    itemCmd.Parameters.AddWithValue("@SalesOrderId", salesOrderId);
                    itemCmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                    itemCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                    itemCmd.Parameters.AddWithValue("@Rate", item.Rate);
                    itemCmd.ExecuteNonQuery();
                }

                tran.Commit();
                return salesOrderId;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }
        public List<SalesOrderModel> GetSalesOrders()
        {
            using var con = GetConnection();
            var cmd = new SqlCommand("PR_SalesOrder_List", con);
            cmd.CommandType = CommandType.StoredProcedure;

            var da = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);

            return dt.AsEnumerable().Select(r => new SalesOrderModel
            {
                SalesOrderId = r.Field<int>("SalesOrderId"),
                OrderNo = r.Field<string>("OrderNo"),
                OrderDate = r.Field<DateTime>("OrderDate"),
                CustomerName = r.Field<string>("CustomerName"),
                CustomerMobile = r.Field<string>("CustomerMobile"),
                SubTotal = r.Field<decimal>("SubTotal"),
                Discount = r.Field<decimal>("Discount"),
                Tax = r.Field<decimal>("Tax"),
                GrandTotal = r.Field<decimal>("GrandTotal"),
                Status = r.Field<string>("Status"),
                CreatedBy = r.Field<int>("CreatedBy")
            }).ToList();
        }

        public SalesOrderModel GetSalesOrderById(int id)
        {
            using var con = GetConnection();
            var cmd = new SqlCommand("PR_SalesOrder_GetById", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@SalesOrderId", id);

            var ds = new DataSet();
            new SqlDataAdapter(cmd).Fill(ds);

            if (ds.Tables[0].Rows.Count == 0)
                return null;

            var orderRow = ds.Tables[0].Rows[0];

            var model = new SalesOrderModel
            {
                SalesOrderId = id,
                OrderNo = orderRow["OrderNo"].ToString(),
                OrderDate = Convert.ToDateTime(orderRow["OrderDate"]),
                CustomerName = orderRow["CustomerName"].ToString(),
                CustomerMobile = orderRow["CustomerMobile"].ToString(),
                SubTotal = Convert.ToDecimal(orderRow["SubTotal"]),
                Discount = Convert.ToDecimal(orderRow["Discount"]),
                Tax = Convert.ToDecimal(orderRow["Tax"]),
                GrandTotal = Convert.ToDecimal(orderRow["GrandTotal"]),
                Status = orderRow["Status"].ToString(),
                CreatedBy = Convert.ToInt32(orderRow["CreatedBy"]),
                Items = new List<SalesOrderItemModel>()
            };

            foreach (DataRow i in ds.Tables[1].Rows)
            {
                model.Items.Add(new SalesOrderItemModel
                {
                    SalesOrderItemId = Convert.ToInt32(i["SalesOrderItemId"]),
                    SalesOrderId = Convert.ToInt32(i["SalesOrderId"]),
                    ProductId = Convert.ToInt32(i["ProductId"]),
                    Quantity = Convert.ToInt32(i["Quantity"]),
                    Rate = Convert.ToDecimal(i["Rate"])
                });
            }

            return model;
        }

        public bool UpdateSalesOrder(SalesOrderModel model)
        {
            using var con = GetConnection();
            var cmd = new SqlCommand("PR_SalesOrder_Update", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@SalesOrderId", model.SalesOrderId);
            cmd.Parameters.AddWithValue("@SubTotal", model.SubTotal);
            cmd.Parameters.AddWithValue("@Discount", model.Discount);
            cmd.Parameters.AddWithValue("@Tax", model.Tax);
            cmd.Parameters.AddWithValue("@GrandTotal", model.GrandTotal);
            cmd.Parameters.AddWithValue("@UpdatedBy", model.CreatedBy);

            con.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
        public bool CancelSalesOrder(int salesOrderId, int updatedBy)
        {
            using var con = GetConnection();
            var cmd = new SqlCommand("PR_SalesOrder_Cancel", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@SalesOrderId", salesOrderId);
            cmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);

            con.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
        public int ConvertToInvoice(int salesOrderId, int createdBy)
        {
            using var con = GetConnection();
            var cmd = new SqlCommand("PR_SalesOrder_ConvertToInvoice", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@SalesOrderId", salesOrderId);
            cmd.Parameters.AddWithValue("@CreatedBy", createdBy);

            con.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
}
