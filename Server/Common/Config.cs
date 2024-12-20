namespace Common;

public class ExecProcess
{
    /// <summary>
    /// 步骤
    /// </summary>
    public SyncProcessStep Step { get; set; }

    /// <summary>
    /// A after B before
    /// </summary>
    public required string StepBeforeOrAfter { get; set; }

    /// <summary>
    /// L Local S Server
    /// </summary>
    public required string ExecInLocalOrServer { get; set; }

    /// <summary>
    /// 执行的应用程序名称
    /// </summary>
    public required string FileName { get; set; }

    /// <summary>
    /// 执行的应用程序参数
    /// </summary>
    public required string Argumnets { get; set; }
}

public class DirFileConfig
{
    /// <summary>
    /// 相对路径,与根路径拼成一个完整的路径，注意 "/" 不要缺少
    /// </summary>
    public required string DirPath { get; set; }

    /// <summary>
    /// 排除的文件，它是根目录的相对路径
    /// </summary>
    public List<string>? Excludes { get; set; }

    /// <summary>
    /// 除此外全部忽略，最高优先级，若有值，ExcludeFiles 将被忽略，它是根目录的相对路径
    /// </summary>
    public List<string>? CherryPicks { get; set; }

    /// <summary>
    /// 本地文件信息，也是即将发布的文件信息，通常是最新的版本
    /// </summary>
    public Dir? LocalDirInfo { get; set; }

    /// <summary>
    /// 差异文件信息
    /// </summary>
    public Dir? DiffDirInfo { get; set; }

    /// <summary>
    /// 远程文件信息，通常是较旧的版本
    /// </summary>
    public Dir? RemoteDirInfo { get; set; }
}

public class MSSqlConfig
{
    /// <summary>
    /// 数据库地址
    /// </summary>
    public required string ServerName { get; set; }

    /// <summary>
    /// db名称
    /// </summary>
    public required string DatabaseName { get; set; }

    /// <summary>
    /// 用户
    /// </summary>
    public required string User { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    public required string Password { get; set; }

    /// <summary>
    /// 通常是：True
    /// </summary>
    public required string TrustServerCertificate { get; set; }

    /// <summary>
    /// 同步数据的表格 ！！！ 通常是 dbo.TableName !!! 注意dbo.
    /// </summary>
    public List<string>? SyncTablesData { get; set; }
}

public class Config
{
    /// <summary>
    /// 发布的项目名称
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 本次发布的唯一id
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 远程Url
    /// </summary>
    public required string RemoteUrl { get; set; }

    /// <summary>
    /// 链接到远程的密码
    /// </summary>
    public required string RemotePwd { get; set; }

    /// <summary>
    /// 是否发布数据库
    /// </summary>
    public required bool IsDeployDb { get; set; }

    /// <summary>
    /// 源数据库连接(ip地址相对LocalServer)
    /// </summary>
    public required MSSqlConfig SrcDb { get; set; }

    /// <summary>
    /// 目标数据库(ip地址相对RemoteServer)
    /// </summary>
    public required MSSqlConfig DstDb { get; set; }

    /// <summary>
    /// 是否发布项目
    /// </summary>
    public required bool IsDeployProject { get; set; }

    /// <summary>
    /// 项目的绝对路径 空字符串表示不发布，不为空LocalRootPath将是发布路径。
    /// </summary>
    public required string LocalProjectAbsolutePath { get; set; }

    /// <summary>
    /// 本地父文件路径
    /// </summary>
    public required string LocalRootPath { get; set; }

    /// <summary>
    /// 远程父路径
    /// </summary>
    public required string RemoteRootPath { get; set; }

    /// <summary>
    /// 同步的文件夹配置
    /// </summary>
    public required List<DirFileConfig> DirFileConfigs { get; set; }

    /// <summary>
    /// 按照步骤执行应用程序扩展列表
    /// </summary>
    public required List<ExecProcess> ExecProcesses { get; set; }
}
