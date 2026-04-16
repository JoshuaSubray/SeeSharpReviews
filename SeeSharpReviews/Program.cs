using Microsoft.EntityFrameworkCore;
using SeeSharpReviews.Data;
using SeeSharpReviews.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ITmdbApiService, TmdbApiService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is missing or empty. " +
        "Set it in appsettings.json, create appsettings.Development.json from appsettings.Development.json.example, " +
        "or run: dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"<your oracle connection string>\"");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(connectionString));
    // options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
    });

builder.Services.AddHttpClient("TMDb", client =>
{
    // Trailing slash is required: otherwise "discover/movie" resolves to /discover/movie (missing /3/).
    var baseUrl = builder.Configuration["TMDb:BaseUrl"] ?? "https://api.themoviedb.org/3/";
    if (!baseUrl.EndsWith('/'))
        baseUrl += "/";
    client.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
