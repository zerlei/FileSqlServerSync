using Microsoft.EntityFrameworkCore;
using RemoteServer.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<SqliteDbContext>(opions=>{
    opions.UseSqlite(builder.Configuration.GetConnectionString("DbPath"));
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseWebSockets();
app.Urls.Clear();
app.Urls.Add("http://0.0.0.0:6828");
app.MapControllers();

app.Run();
