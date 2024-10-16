using DataNex.Data;
using DataNexApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using DataNex.Model.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Configuration;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
 {
     options.JsonSerializerOptions.PropertyNamingPolicy = null;
 });
builder.Services.AddCors();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var provider = builder.Configuration.GetSection("Provider").Value;
    if (provider == "MsSQL")
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection"), x => x.MigrationsAssembly("DataNex.Data"));
        options.UseSqlServer(x => x.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
        AppBase.ConnectionString =builder.Configuration.GetConnectionString("DbConnection");
    }
    else if (provider == "MySQL")
    {
        options.UseMySql(builder.Configuration.GetConnectionString("MySqlConnection"), ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySqlConnection")));
    }
});


builder.Services.AddIdentity<User, Roles>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


// Configure authentication
builder.Services.AddAuthentication(opt =>
{
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


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddExceptionHandler<DataNexApiExceptionHandler>();
builder.Services.AddAutoMapper(typeof(Program));
// Add services to the container.
var app = builder.Build();

//Seed data on Startup
var applicationDataSeeder = new ApplicationDataSeeder();
await applicationDataSeeder.Seed(app.Services);

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

if (origins.Length > 0 && !string.IsNullOrEmpty(origins[0]))
{
    app.UseCors(x => x.AllowAnyMethod().AllowAnyHeader().WithOrigins(origins).AllowCredentials());
}
else
{
    app.UseCors(x => x.SetIsOriginAllowed(origin => true).AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
}

app.UseAuthentication();
app.UseAuthorization();

app.UseExceptionHandler(o => { });

app.MapControllers();
await app.RunAsync();
