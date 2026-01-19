using Inventory_Management_System_with_Sales_Management_API.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Inventory_Management_System_with_Sales_Management_API.Data
{
    public class SalesRepository : ISalesRepository
    {
        private readonly IConfiguration _configuration;

        public SalesRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection()
            => new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        // ================= CREATE SALE =================
        public int CreateSale(SaleCreateModel model)
        {
            using var con = GetConnection();
            con.Open();
            var tran = con.BeginTransaction();

            try
            {
                // 1️⃣ Generate Invoice No
                string invoiceNo;

                using (SqlCommand cmd = new SqlCommand("PR_Sales_GenerateInvoiceNo", con, tran))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    SqlParameter outputParam = new SqlParameter("@InvoiceNo", SqlDbType.NVarChar, 50)
                    {
                        Direction = ParameterDirection.Output
                    };

                    cmd.Parameters.Add(outputParam);

                    cmd.ExecuteNonQuery();

                    invoiceNo = outputParam.Value.ToString();
                }

                // 2️⃣ Insert Sale (🔥 FIXED → CustomerId)
                int saleId;
                using (var cmd = new SqlCommand("PR_Sales_Insert", con, tran))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@InvoiceNo", invoiceNo);
                    cmd.Parameters.AddWithValue("@InvoiceDate", model.InvoiceDate);
                    cmd.Parameters.AddWithValue("@CustomerId", model.CustomerId); // ✅ FIX
                    cmd.Parameters.AddWithValue("@SubTotal", model.SubTotal);
                    cmd.Parameters.AddWithValue("@Discount", model.Discount);
                    cmd.Parameters.AddWithValue("@Tax", model.Tax);
                    cmd.Parameters.AddWithValue("@GrandTotal", model.GrandTotal);
                    cmd.Parameters.AddWithValue("@PaymentMode", model.PaymentMode ?? "Cash");
                    cmd.Parameters.AddWithValue("@CreatedBy", model.CreatedBy);

                    saleId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // 3️⃣ Insert Items
                foreach (var item in model.Items)
                {
                    using (var cmd = new SqlCommand("PR_SaleItems_Insert", con, tran))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@SaleId", saleId);
                        cmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                        cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                        cmd.Parameters.AddWithValue("@Rate", item.Rate);
                        cmd.Parameters.AddWithValue("@CreatedBy", model.CreatedBy);
                        cmd.ExecuteNonQuery();
                    }

                    // 4️⃣ Stock Transaction
                    using (var cmd = new SqlCommand("PR_StockTransaction_Sale", con, tran))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                        cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                        cmd.Parameters.AddWithValue("@SaleId", saleId);
                        cmd.Parameters.AddWithValue("@CreatedBy", model.CreatedBy);
                        cmd.ExecuteNonQuery();
                    }
                }

                tran.Commit();
                return saleId;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        // ================= SALES LIST =================
        public List<SaleListModel> GetSales()
        {
            var list = new List<SaleListModel>();

            using var con = GetConnection();
            using var cmd = new SqlCommand("PR_Sales_List", con);
            cmd.CommandType = CommandType.StoredProcedure;

            con.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new SaleListModel
                {
                    SaleId = Convert.ToInt32(dr["SaleId"]),
                    InvoiceNo = dr["InvoiceNo"].ToString(),
                    InvoiceDate = Convert.ToDateTime(dr["InvoiceDate"]),
                    CustomerName = dr["CustomerName"].ToString(), // from JOIN
                    GrandTotal = Convert.ToDecimal(dr["GrandTotal"]),
                    PaymentStatus = dr["PaymentStatus"].ToString(),
                    CreatedBy = dr["CreatedBy"].ToString()
                });
            }

            return list;
        }

        // ================= INVOICE VIEW =================
        public InvoiceViewModel GetInvoice(int saleId)
        {
            using var con = GetConnection();
            using var cmd = new SqlCommand("PR_Sales_GetInvoice", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@SaleId", saleId);

            con.Open();
            using var dr = cmd.ExecuteReader();

            InvoiceViewModel invoice = null;

            // HEADER
            if (dr.Read())
            {
                invoice = new InvoiceViewModel
                {
                    SaleId = saleId,
                    InvoiceNo = dr["InvoiceNo"].ToString(),
                    InvoiceDate = Convert.ToDateTime(dr["InvoiceDate"]),
                    CustomerId = Convert.ToInt32(dr["CustomerId"]),
                    CustomerName = dr["CustomerName"].ToString(),
                    CustomerMobile = dr["CustomerMobile"].ToString(),
                    SubTotal = Convert.ToDecimal(dr["SubTotal"]),
                    Discount = Convert.ToDecimal(dr["Discount"]),
                    Tax = Convert.ToDecimal(dr["Tax"]),
                    GrandTotal = Convert.ToDecimal(dr["GrandTotal"]),
                    PaymentMode = dr["PaymentMode"].ToString(),
                    PaymentStatus = dr["PaymentStatus"].ToString(),
                    Items = new List<InvoiceItemViewModel>()
                };
            }

            // ITEMS
            if (dr.NextResult())
            {
                while (dr.Read())
                {
                    invoice.Items.Add(new InvoiceItemViewModel
                    {
                        SaleItemId = Convert.ToInt32(dr["SaleItemId"]),
                        ProductName = dr["ProductName"].ToString(),
                        Quantity = Convert.ToInt32(dr["Quantity"]),
                        Rate = Convert.ToDecimal(dr["Rate"]),
                        Amount = Convert.ToDecimal(dr["Amount"])
                    });
                }
            }

            return invoice;
        }
    }
}
