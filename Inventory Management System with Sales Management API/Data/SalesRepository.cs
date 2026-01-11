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

        public int CreateSale(SaleCreateModel model)
        {
            using SqlConnection con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            con.Open();
            SqlTransaction tran = con.BeginTransaction();

            try
            {
                // 1️⃣ Generate Invoice No
                string invoiceNo;
                using (SqlCommand cmd = new SqlCommand("PR_Sales_GenerateInvoiceNo", con, tran))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    invoiceNo = cmd.ExecuteScalar().ToString();
                }

                // 2️⃣ Insert Sale
                int saleId;
                using (SqlCommand cmd = new SqlCommand("PR_Sales_Insert", con, tran))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@InvoiceNo", invoiceNo);
                    cmd.Parameters.AddWithValue("@InvoiceDate", model.InvoiceDate);
                    cmd.Parameters.AddWithValue("@CustomerName", model.CustomerName);
                    cmd.Parameters.AddWithValue("@CustomerMobile", model.CustomerMobile ?? "");
                    cmd.Parameters.AddWithValue("@SubTotal", model.SubTotal);
                    cmd.Parameters.AddWithValue("@Discount", model.Discount);
                    cmd.Parameters.AddWithValue("@Tax", model.Tax);
                    cmd.Parameters.AddWithValue("@GrandTotal", model.GrandTotal);
                    cmd.Parameters.AddWithValue("@PaymentMode", model.PaymentMode ?? "");
                    cmd.Parameters.AddWithValue("@CreatedBy", model.CreatedBy);

                    saleId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // 3️⃣ Insert Items + Stock
                foreach (var item in model.Items)
                {
                    using (SqlCommand cmd = new SqlCommand("PR_SaleItems_Insert", con, tran))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@SaleId", saleId);
                        cmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                        cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                        cmd.Parameters.AddWithValue("@Rate", item.Rate);
                        cmd.ExecuteNonQuery();
                    }

                    using (SqlCommand cmd = new SqlCommand("PR_StockTransaction_Sale", con, tran))
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

        public List<SaleListModel> GetSales()
        {
            List<SaleListModel> list = new();

            using SqlConnection con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            using SqlCommand cmd = new SqlCommand("PR_Sales_List", con);
            cmd.CommandType = CommandType.StoredProcedure;

            con.Open();
            using SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new SaleListModel
                {
                    SaleId = Convert.ToInt32(dr["SaleId"]),
                    InvoiceNo = dr["InvoiceNo"].ToString(),
                    InvoiceDate = Convert.ToDateTime(dr["InvoiceDate"]),
                    CustomerName = dr["CustomerName"].ToString(),
                    GrandTotal = Convert.ToDecimal(dr["GrandTotal"]),
                    PaymentStatus = dr["PaymentStatus"].ToString(),
                    CreatedBy = dr["CreatedBy"].ToString()
                });
            }

            return list;
        }

        public InvoiceViewModel GetInvoice(int saleId)
        {
            using SqlConnection con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            using SqlCommand cmd = new SqlCommand("PR_Sales_GetInvoice", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@SaleId", saleId);

            con.Open();

            using SqlDataReader dr = cmd.ExecuteReader();

            InvoiceViewModel invoice = null;

            // ================= HEADER =================
            if (dr.Read())
            {
                invoice = new InvoiceViewModel
                {
                    SaleId = Convert.ToInt32(dr["SaleId"]),
                    InvoiceNo = dr["InvoiceNo"].ToString(),
                    InvoiceDate = Convert.ToDateTime(dr["InvoiceDate"]),
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

            // ================= ITEMS =================
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
