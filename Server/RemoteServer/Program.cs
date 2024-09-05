using Microsoft.EntityFrameworkCore;
using RemoteServer;
using RemoteServer.Models;

var builder = WebApplication.CreateBuilder(args);

ConfigurationBuilder configurationBuilder = new();

// Add services to the container.

//添加配置文件路径
configurationBuilder
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json");

//加载文件
IConfiguration _configuration = configurationBuilder.Build();
RemoteSyncServer.TempRootFile = _configuration["TempDir"] ?? "C:/TempPack";
;
builder.Services.AddControllers();
builder.Services.AddDbContext<SqliteDbContext>(opions =>
{
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
app.Urls.Add("http://0.0.0.0:6818");
app.MapControllers();

app.Run();
