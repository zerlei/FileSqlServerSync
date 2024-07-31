namespace LocalServer.Models;

public class DirFileConfig
{
    /// <summary>
    /// 本地-源根目录
    /// </summary>
    public required string LocalRootPath { get; set; }

    /// <summary>
    /// 远程-目标根目录
    /// </summary>
    public required string RemoteRootPath { get; set; }

    /// <summary>
    /// 排除的文件，它是根目录的相对路径
    /// </summary>
    public List<string>? ExcludeFiles { get; set; }

    /// <summary>
    /// 除此外全部忽略，最高优先级，若有值，ExcludeFiles 将被忽略，它是根目录的相对路径
    /// </summary>
    public List<DirFileConfig>? CherryPickFiles { get; set; }
}

public class Config
{
    public required string Name { get; set; }
    public required string RemoteUrl { get; set; }

    public List<DirFileConfig>? DirFileConfigs { get; set; }
}
