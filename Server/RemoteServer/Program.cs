using Microsoft.EntityFrameworkCore;
using RemoteServer;
using RemoteServer.Models;

var builder = WebApplication.CreateBuilder(args);

ConfigurationBuilder configurationBuilder = new();

// Add services to the container.

//添加配置文件路径
RemoteSyncServerFactory.NamePwd = [.. (
    builder.Configuration.GetSection("NamePwds").Get<Tuple<string, string>[]>() ?? []
)];
RemoteSyncServer.TempRootFile = builder.Configuration["TempDir"] ?? "C:/TempPack";
builder.Services.AddControllers();
builder.Services.AddDbContext<SqliteDbContext>(opions =>
{
    opions.UseSqlite(builder.Configuration.GetConnectionString("DbPath"));
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<RemoteSyncServerFactory>();
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
