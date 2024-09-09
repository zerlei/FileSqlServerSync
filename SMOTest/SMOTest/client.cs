using System;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace DatabaseStructureOnlyExample
{
    class Program
    {
        static void Main(string[] args)
        {
            string serverName = "your_server_name";
            string sourceDatabaseName = "source_database_name";
            string targetDatabaseName = "target_database_name";

            // 连接到 SQL Server 实例
            ServerConnection serverConnection = new ServerConnection(serverName);
            Server server = new Server(serverConnection);

            // 创建目标数据库
            Database targetDatabase = new Database(server, targetDatabaseName);
            targetDatabase.Create();

            // 获取源数据库
            Database sourceDatabase = server.Databases[sourceDatabaseName];

            // 创建一个脚本生成器
            Scripter scripter = new Scripter(server)
            {
                Options =
                {
                    ScriptDrops = false, // 不生成删除脚本
                    WithDependencies = true, // 包括所有相关对象
                    IncludeIfNotExists = false, // 不包含如果不存在的语句
                    IncludeSchema = true, // 包含架构
                    IncludeData = false // 不包含数据
                }
            };

            // 生成表的创建脚本
            foreach (Table table in sourceDatabase.Tables)
            {
                if (!table.IsSystemObject)
                {
                    var script = scripter.Script(new Urn[] { table.Urn });
                    foreach (var line in script)
                    {
                        // 将脚本执行到目标数据库
                        server.ConnectionContext.ExecuteNonQuery(line);
                    }
                }
            }

            Console.WriteLine($"结构仅副本 '{targetDatabaseName}' 创建成功。");
        }
    }
}
