using DataNex.Data;
using DataNexApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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
});
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
    app.UseCors(x=>x.AllowAnyMethod().AllowAnyHeader().WithOrigins(origins));
}
else
{
    app.UseCors(x => x.SetIsOriginAllowed(origin => true).AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
}
app.UseAuthorization();

app.UseExceptionHandler(o => { });

app.MapControllers();

await app.RunAsync();
