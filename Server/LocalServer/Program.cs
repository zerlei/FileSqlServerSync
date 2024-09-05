using LocalServer;

var builder = WebApplication.CreateBuilder(args);
ConfigurationBuilder configurationBuilder = new ();

//添加配置文件路径
configurationBuilder.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");

//加载文件
IConfiguration _configuration = configurationBuilder.Build();
LocalSyncServer.TempRootFile = _configuration["TempDir"]??"C:/TempPack";;
// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<LocalSyncServerFactory>();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseWebSockets();
app.UseAuthorization();
app.Urls.Clear();
app.Urls.Add("http://0.0.0.0:6818");
app.MapControllers();

app.Run();
