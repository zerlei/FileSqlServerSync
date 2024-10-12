using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Validations;
using RemoteServer;
using RemoteServer.Models;

var builder = WebApplication.CreateBuilder(args);

ConfigurationBuilder configurationBuilder = new();

// Add services to the container.

//添加配置文件路径
foreach (var x in builder.Configuration.GetSection("NamePwds").GetChildren())
{
    var it = x.GetChildren();
    RemoteSyncServerFactory.NamePwd.Add(
        new Tuple<string, string>(it.ElementAt(0).Value ?? "", it.ElementAt(1).Value ?? "")
    );
}
RemoteSyncServer.SqlPackageAbPath =
    builder.Configuration["SqlPackageAbPath"]
    ?? "C:\\Users\\ZHAOLEI\\.dotnet\\tools\\sqlpackage.exe";
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
app.Urls.Add("http://0.0.0.0:6819");
app.MapControllers();
app.Run();
