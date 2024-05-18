using DataNex.Data;
using DataNexApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using MySQL.Data.EntityFrameworkCore;
using System.Configuration;
using Microsoft.AspNetCore.Identity;
using DataNex.Model.Models;
using DataNex.Model.Dtos;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
 {
     options.JsonSerializerOptions.PropertyNamingPolicy = null;
 });
builder.Services.AddCors();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
     options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection"), x => x.MigrationsAssembly("DataNex.Data"));
   // options.UseMySql(builder.Configuration.GetConnectionString("MySqlConnection"), ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySqlConnection")));
});

builder.Services.AddAuthentication(opt => {
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "http://localhost:5000",
            ValidAudience = "http://localhost:5000",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("=d*T-pAtiG-cEID=&8,^XVTSNE50.)|6Ch(PM~L&`A'y(mChC_.2mR|,h]-TM~9.Z$Pam.gz]ZH)HwP`!setATBPaV^2Wlq+~kdohCDo`H0BC8i[U}PY>V.fqHhZ#O"))
        };
    });
// Configure authentication
//builder.Services.AddAuthorization();
//builder.Services.AddAuthentication().AddCookie(options =>
//{
//    options.Cookie.SameSite = SameSiteMode.None; // Allow cross-site cookies
//    options.Cookie.HttpOnly = true;
//    options.Cookie.Path = "/";
//    options.Cookie.HttpOnly = true;
//    options.Cookie.IsEssential = true;


//});
//builder.Services.AddIdentityCore<User>().AddEntityFrameworkStores<ApplicationDbContext>().AddApiEndpoints();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddExceptionHandler<DataNexApiExceptionHandler>();
builder.Services.AddAutoMapper(typeof(Program));
// Add services to the container.
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    var useHttps = builder.Configuration.GetRequiredSection("UseHttps").Get<bool>();
    if (useHttps)
    {
        app.UseHttpsRedirection();
        app.UseHsts();
    }

}

var origins = builder.Configuration["AllowedOrigins"].Split(";");

if(origins.Length > 0 && !string.IsNullOrEmpty(origins[0]))
{
    app.UseCors(x=>x.AllowAnyMethod().AllowAnyHeader().WithOrigins(origins).AllowCredentials());
}
else
{
    app.UseCors(x => x.SetIsOriginAllowed(origin => true).AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
}

app.UseAuthentication();
app.UseAuthorization();

app.UseExceptionHandler(o => { });

app.MapControllers();
//app.MapIdentityApi<User>();
await app.RunAsync();
