namespace Common;

public class DirFileConfig
{
    /// <summary>
    /// 相对路径
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
    public Dir? DirInfo { get; set; }
}

public class Config
{
    /// <summary>
    /// 发布的项目名称
    /// </summary>
    public required string Name { get; set; }

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
    /// 源数据库连接字符串(ip地址相对LocalServer)
    /// </summary>
    public required string SrcDbConnection { get; set; }

    /// <summary>
    /// 目标数据库连接字符串(ip地址相对RemoteServer)
    /// </summary>
    public required string DstDbConnection { get; set; }

    /// <summary>
    /// 同步的表
    /// </summary>
    public required List<string>? SyncDataTables { get; set; }

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
}
