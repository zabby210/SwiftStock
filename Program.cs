var builder = WebApplication.CreateBuilder(args);

// Enable session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // ✅ Extends session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();

var CORSPolicy = "_CORSPolicy";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: CORSPolicy,
        CORSPolicy =>
        {
            CORSPolicy.AllowAnyHeader().
            AllowAnyMethod().
            WithOrigins(
                "https://localhost:7044",
                "http://localhost:5020"
                );
        }
        );
});

var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();

app.UseCors(CORSPolicy); // ✅ Enables CORS if frontend is separate

app.UseSession(); // ✅ Sessions should be enabled after routing

app.UseAuthorization(); // ✅ Ensures role-based access (if needed)

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapFallbackToFile("cashier.html");
});

app.Run();
