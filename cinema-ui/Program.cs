using cinema_ui.Services;
using cinema_ui.Filters;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient<ApiService>();
builder.Services.AddHttpClient<AdminApiService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ApiService>();
builder.Services.AddScoped<AdminApiService>();

// Configure JWT cookie authentication
builder.Services.AddAuthentication("JwtCookie")
    .AddScheme<AuthenticationSchemeOptions, JwtCookieAuthenticationHandler>(
        "JwtCookie", options => { });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
