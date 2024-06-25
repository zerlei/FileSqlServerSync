using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RemoteServer;

public class SyncLogHead
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// git commitID
    /// </summary>
    [MaxLength(50)]
    public required string CommitID { get; set; }

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
    public string? Message {get;set;}
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
