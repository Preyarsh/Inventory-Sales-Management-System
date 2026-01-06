using Inventory_Management_System_with_Sales_Management_MVC.Service;

var builder = WebApplication.CreateBuilder(args);

// =======================
// ADD SERVICES (ONLY HERE)
// =======================
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();
builder.Services.AddSession();

// ✅ Register ProductService with HttpClient
builder.Services.AddHttpClient<ProductService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:44330/"); // API URL
});

// =======================
// BUILD (ONLY ONCE)
// =======================

var app = builder.Build();

// =======================
// MIDDLEWARE PIPELINE
// =======================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();
