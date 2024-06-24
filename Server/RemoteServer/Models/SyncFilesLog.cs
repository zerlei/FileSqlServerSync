using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RemoteServer;

public class SyncLogHead
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// git versions
    /// </summary>
    [MaxLength(50)]
    public required string VersionsFromTag { get; set; }

    /// <summary>
    /// 同步时间
    /// </summary>
    public DateTime SyncTime { get; set; }

    /// <summary>
    /// 客户端ID
    /// </summary>
    [MaxLength(200)]
    public required string ClientID { get; set; }

    /// <summary>
    /// 客户端名称
    /// </summary>
    [MaxLength(50)]
    public string? ClientName { get; set; }

    /// <summary>
    /// 状态 0 正在进行，1 已完成，2 失败有错误
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// 同步消息
    /// </summary>
    public string? Message { get; set; }
}

public class SyncLogFile
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// 头部Id
    /// </summary>
    public Guid HeadId { get; set; }

    /// <summary>
    /// 客户端文件全目录
    /// </summary>
    [MaxLength(500)]
    public required string ClientRootPath { get; set; }

    /// <summary>
    /// 服务器文件全目录
    /// </summary>
    [MaxLength(500)]
    public required string ServerRootPath { get; set; }

    /// <summary>
    /// 相对路径
    /// </summary>
    [MaxLength(500)]
    public required string RelativePath { get; set; }
}

public class SyncGitCommit
{
    [Key]
    public Guid Id { get; set; }
    public Guid HeadId { get; set; }
    /// <summary>
    /// git commit id
    /// </summary>
    public required string CommitId { get; set; }
    /// <summary>
    /// git commit 用户名
    /// </summary>
    public required string CommitUserName { get; set; }
    /// <summary>
    /// git commit 时间
    /// </summary>
    public DateTime CommitTime { get; set; }
    /// <summary>
    /// git 提交内容
    /// </summary>
    public required string CommitMessage { get; set; }
}
