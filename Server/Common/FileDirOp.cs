using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.VisualBasic;

namespace Common;

/// <summary>
/// 文件操作策略
/// </summary>
public abstract class FileDirOpStra
{
    public abstract void FileCreate(string absolutePath, DateTime mtime);

    public abstract void DirCreate(Dir dir, bool IsRecursion = true);

    public abstract void FileModify(string absolutePath, DateTime mtime);
    public abstract void FileDel(string absolutePath);
    public abstract void DirDel(Dir dir, bool IsRecursion = true);
}

/// <summary>
/// 文件目录打包
/// </summary>
/// <param name="dstRootPath"></param>
public class FileDirOpForPack(string srcRootPath, string dstRootPath) : FileDirOpStra
{
    /// <summary>
    /// 目标根目录
    /// </summary>
    public readonly string DstRootPath = dstRootPath;

    /// <summary>
    /// 源目录
    /// </summary>
    public readonly string SrcRootPath = srcRootPath;

    /// <summary>
    /// 最终完成时的压缩
    /// </summary>
    public void FinallyCompress()
    {
        var x = DstRootPath;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="absolutePath"></param>
    /// <param name="mtime"></param>
    /// <exception cref="NotImplementedException"></exception>
    public override void FileCreate(string srcPath, DateTime mtime)
    {
        var dstPath = srcPath.Replace(SrcRootPath, DstRootPath);

        var dstDirPath =
            Path.GetDirectoryName(dstPath) ?? throw new NullReferenceException("父路径不存在！");
        if (!Directory.Exists(dstDirPath))
        {
            Directory.CreateDirectory(dstDirPath);
        }

        System.IO.File.Copy(srcPath, dstPath, true);
    }

    public override void DirCreate(Dir dir, bool IsRecursion = true)
    {
        var srcPath = dir.FormatedPath;
        var dstPath = srcPath.Replace(SrcRootPath, DstRootPath);
        var dstDirPath =
            Path.GetDirectoryName(dstPath) ?? throw new NullReferenceException("父路径不存在！");
        if (!Directory.Exists(dstDirPath))
        {
            Directory.CreateDirectory(dstDirPath);
        }
        if (IsRecursion)
        {
            foreach (var c in dir.Children)
            {
                if (c is Dir d)
                {
                    this.DirCreate(d, true);
                }
                else if (c is File f)
                {
                    this.FileCreate(f.FormatedPath, f.MTime);
                }
            }
        }
    }

    public override void FileModify(string absolutePath, DateTime mtime)
    {
        this.FileCreate(absolutePath, mtime);
    }

    public override void FileDel(string absolutePath)
    {
        throw new NotImplementedException();
    }

    public override void DirDel(Dir dir, bool IsRecursion = true)
    {
        throw new NotImplementedException();
    }
}

public class FileDirOpForUnpack(string srcCompressedPath, string dstRootPath) : FileDirOpStra
{
    /// <summary>
    /// 解压缩,必须首先调用
    /// </summary>
    public void FirstUnComparess()
    {
        var x = SrcCompressedPath;
    }

    /// <summary>
    /// 目标根目录
    /// </summary>
    public readonly string DstRootPath = dstRootPath;

    /// <summary>
    /// 源目录
    /// </summary>
    public readonly string SrcCompressedPath = srcCompressedPath;

    /// <summary>
    /// 最终完成时的压缩
    /// </summary>
    public override void FileCreate(string absolutePath, DateTime mtime)
    {
        throw new NotImplementedException();
    }

    public override void DirCreate(Dir dir, bool IsRecursion = true)
    {
        throw new NotImplementedException();
    }

    public override void FileModify(string absolutePath, DateTime mtime)
    {
        throw new NotImplementedException();
    }

    public override void FileDel(string absolutePath)
    {
        throw new NotImplementedException();
    }

    public override void DirDel(Dir dir, bool IsRecursion = true)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// 文件目录权限校验
/// </summary>
public class FileDirOpForAccessCheck : FileDirOpStra
{
    public override void FileCreate(string absolutePath, DateTime mtime)
    {
        throw new NotImplementedException();
    }

    public override void DirCreate(Dir dir, bool IsRecursion = true)
    {
        throw new NotImplementedException();
    }

    public override void FileModify(string absolutePath, DateTime mtime)
    {
        throw new NotImplementedException();
    }

    public override void FileDel(string absolutePath)
    {
        throw new NotImplementedException();
    }

    public override void DirDel(Dir dir, bool IsRecursion = true)
    {
        throw new NotImplementedException();
    }
}

public enum FileAccess
{
    Read,
    Write,
    Delete,
    Execute
}

public enum DirAcess
{
    /// <summary>
    /// 读取权限
    /// </summary>
    Read,

    /// <summary>
    /// 写入权限
    /// </summary>
    Write,

    /// <summary>
    /// 列出文件夹权限
    /// </summary>
    ListDirectory,

    /// <summary>
    /// 创建文件权限
    /// </summary>
    CreateFiles,

    /// <summary>
    /// 创建文件夹权限
    /// </summary>
    CreateDirectories,

    /// <summary>
    /// 删除文件权限
    /// </summary>
    Delete,

    /// <summary>
    /// 删除文件夹及其子文件
    /// </summary>
    DeleteSubdirectoriesAndFiles,
}

/// <summary>
/// 运行此软件的用户与目标软件的用户最好是 一个用户，一个用户组，或者运行此软件的用户具备最高权限。
/// </summary>
public class AccessWrapper
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="absolutePath"></param>
    public static void FreeThisDirAccess(string absolutePath)
    {
        if (
            CheckDirAccess(
                absolutePath,
                [
                    DirAcess.Read,
                    DirAcess.Write,
                    DirAcess.Delete,
                    DirAcess.ListDirectory,
                    DirAcess.CreateFiles,
                    DirAcess.CreateDirectories,
                    DirAcess.DeleteSubdirectoriesAndFiles
                ]
            )
        ) { }
        else
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                DirectoryInfo dirInfo = new(absolutePath);
                //获得该文件的访问权限
                var dirSecurity = dirInfo.GetAccessControl();
                //设定文件ACL继承
                InheritanceFlags inherits =
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;

                var cUser =
                    (
                        WindowsIdentity.GetCurrent().Groups
                        ?? throw new Exception("GetWindowsIdentity failed. 你需要手动处理发布内容！--检查权限！")
                    ).FirstOrDefault() ?? throw new NullReferenceException("can't be null");
                FileSystemAccessRule MdfRule =
                    new(
                        cUser,
                        FileSystemRights.Modify,
                        inherits,
                        PropagationFlags.InheritOnly,
                        AccessControlType.Allow
                    );
                if (dirSecurity.ModifyAccessRule(AccessControlModification.Set, MdfRule, out _)) { }
                else
                {
                    throw new Exception("AddAccessRule failed. 你需要手动处理发布内容！--检查权限!");
                }
                //设置访问权限
                dirInfo.SetAccessControl(dirSecurity);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                //TODO  Linux文件权限
            }
            else
            {
                throw new NotSupportedException(
                    $"{RuntimeInformation.OSDescription} is not supported."
                );
            }
        }
    }

    public static void FreeThisFileAccess(string absolutePath)
    {
        if (
            CheckFileAccess(
                absolutePath,
                [FileAccess.Read, FileAccess.Write, FileAccess.Delete, FileAccess.Execute]
            )
        ) { }
        else
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                FileInfo fileInfo = new(absolutePath);
                //获得该文件的访问权限
                FileSecurity fileSecurity = fileInfo.GetAccessControl();

                var cUser =
                    (
                        WindowsIdentity.GetCurrent().Groups
                        ?? throw new NullReferenceException(
                            "GetWindowsIdentity failed. 你需要手动处理发布内容！-- 检查权限"
                        )
                    ).FirstOrDefault() ?? throw new NullReferenceException("can't be null");
                FileSystemAccessRule MdfRule =
                    new(cUser, FileSystemRights.Modify, AccessControlType.Allow);
                if (fileSecurity.ModifyAccessRule(AccessControlModification.Set, MdfRule, out _))
                { }
                else
                {
                    throw new Exception("AddAccessRule failed. 你需要手动处理发布内容！--检查权限");
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                //TODO  Linux文件权限
            }
            else
            {
                throw new NotSupportedException(
                    $"{RuntimeInformation.OSDescription} is not supported."
                );
            }
        }
    }

    public static bool CheckDirAccess(string absolutePath, DirAcess[] access)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            DirectoryInfo dirInfo = new(absolutePath);
            //获得该文件的访问权限
            var dirSecurity = dirInfo.GetAccessControl();
            var ac = dirSecurity
                .GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier))
                .Cast<FileSystemAccessRule>();
#pragma warning disable CA1416 // Validate platform compatibility
            var it =
                from i in ac
                where
                    (
                        WindowsIdentity.GetCurrent().Groups
                        ?? throw new NullReferenceException("未能获取当前用户组！")
                    ).Contains(i.IdentityReference)
                select i;
#pragma warning restore CA1416 // Validate platform compatibility
            List<DirAcess> caccess = [];
            foreach (var i in it)
            {
                if (i.FileSystemRights.HasFlag(FileSystemRights.FullControl))
                {
                    return true;
                }
                if (
                    i.FileSystemRights.HasFlag(
                        FileSystemRights.Read
                            | FileSystemRights.Modify
                            | FileSystemRights.ReadAndExecute
                    )
                )
                {
                    caccess.Add(DirAcess.Read);
                }
                if (i.FileSystemRights.HasFlag(FileSystemRights.Write | FileSystemRights.Modify))
                {
                    caccess.Add(DirAcess.Write);
                }
                if (i.FileSystemRights.HasFlag(FileSystemRights.Modify | FileSystemRights.Delete))
                {
                    caccess.Add(DirAcess.Delete);
                }
                if (
                    i.FileSystemRights.HasFlag(
                        FileSystemRights.ListDirectory | FileSystemRights.Modify
                    )
                )
                {
                    caccess.Add(DirAcess.ListDirectory);
                }
                if (
                    i.FileSystemRights.HasFlag(
                        FileSystemRights.CreateFiles | FileSystemRights.Modify
                    )
                )
                {
                    caccess.Add(DirAcess.CreateFiles);
                }
                if (
                    i.FileSystemRights.HasFlag(
                        FileSystemRights.CreateDirectories | FileSystemRights.Modify
                    )
                )
                {
                    caccess.Add(DirAcess.CreateDirectories);
                }
                if (
                    i.FileSystemRights.HasFlag(
                        FileSystemRights.DeleteSubdirectoriesAndFiles | FileSystemRights.Modify
                    )
                )
                {
                    caccess.Add(DirAcess.DeleteSubdirectoriesAndFiles);
                }
            }
            foreach (var i in access)
            {
                if (!caccess.Contains(i))
                {
                    return false;
                }
            }
            return true;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            //TODO  Linux文件权限
            return true;
        }
        else
        {
            throw new NotSupportedException(
                $"{RuntimeInformation.OSDescription} is not supported."
            );
        }
    }

    public static bool CheckFileAccess(string absolutePath, FileAccess[] access)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            FileInfo fileInfo = new(absolutePath);
            //获得该文件的访问权限
            FileSecurity fileSecurity = fileInfo.GetAccessControl();

            var ac = fileSecurity
                .GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier))
                .Cast<FileSystemAccessRule>();
#pragma warning disable CA1416 // 验证平台兼容性
            var it =
                from i in ac
                where
                    (
                        WindowsIdentity.GetCurrent().Groups
                        ?? throw new NullReferenceException("未能获取当前用户组！")
                    ).Contains(i.IdentityReference)
                select i;
#pragma warning restore CA1416 // 验证平台兼容性

            List<FileAccess> caccess = [];
            foreach (var i in it)
            {
                if (i.FileSystemRights.HasFlag(FileSystemRights.FullControl))
                {
                    return true;
                }
                if (i.FileSystemRights.HasFlag(FileSystemRights.Read | FileSystemRights.Modify))
                {
                    caccess.Add(FileAccess.Read);
                }
                if (i.FileSystemRights.HasFlag(FileSystemRights.Write | FileSystemRights.Modify))
                {
                    caccess.Add(FileAccess.Write);
                }
                if (i.FileSystemRights.HasFlag(FileSystemRights.Write | FileSystemRights.Modify))
                {
                    caccess.Add(FileAccess.Delete);
                }
                if (
                    i.FileSystemRights.HasFlag(
                        FileSystemRights.ExecuteFile | FileSystemRights.Modify
                    )
                )
                {
                    caccess.Add(FileAccess.Execute);
                }
            }
            foreach (var i in access)
            {
                if (!caccess.Contains(i))
                {
                    return false;
                }
            }
            return true;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            //TODO  Linux文件夹权限
            return true;
        }
        else
        {
            throw new NotSupportedException(
                $"{RuntimeInformation.OSDescription} is not supported."
            );
        }
    }

    /// <summary>
    /// 获取当前用户
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>

    public static string GetCurrentUser()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return System.Security.Principal.WindowsIdentity.GetCurrent().Name;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Environment.UserName;
        }
        else
        {
            throw new NotSupportedException(
                $"{RuntimeInformation.OSDescription} is not supported."
            );
        }
    }
}
