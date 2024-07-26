using System.Linq;
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
    /// 修改权限
    /// </summary>
    Modify,

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
                    DirAcess.Modify,
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
                    WindowsIdentity.GetCurrent().User
                    ?? throw new Exception("GetWindowsIdentity failed. 你需要手动处理发布内容！");
                FileSystemAccessRule ReadRule =
                    new(
                        cUser,
                        FileSystemRights.Read,
                        inherits,
                        PropagationFlags.None,
                        AccessControlType.Allow
                    );
                FileSystemAccessRule WriteRule =
                    new(
                        cUser,
                        FileSystemRights.Write,
                        inherits,
                        PropagationFlags.None,
                        AccessControlType.Allow
                    );
                FileSystemAccessRule ModifyRule =
                    new(
                        cUser,
                        FileSystemRights.Modify,
                        inherits,
                        PropagationFlags.None,
                        AccessControlType.Allow
                    );
                FileSystemAccessRule DeleteRule =
                    new(
                        cUser,
                        FileSystemRights.Delete,
                        inherits,
                        PropagationFlags.None,
                        AccessControlType.Allow
                    );
                FileSystemAccessRule ListDirectoryRule =
                    new(
                        cUser,
                        FileSystemRights.ListDirectory,
                        inherits,
                        PropagationFlags.None,
                        AccessControlType.Allow
                    );
                FileSystemAccessRule CreateFilesRule =
                    new(
                        cUser,
                        FileSystemRights.CreateFiles,
                        inherits,
                        PropagationFlags.None,
                        AccessControlType.Allow
                    );
                FileSystemAccessRule CreateDirsRule =
                    new(
                        cUser,
                        FileSystemRights.CreateDirectories,
                        inherits,
                        PropagationFlags.None,
                        AccessControlType.Allow
                    );
                FileSystemAccessRule DeleteSubdirectoriesAndFilesRule =
                    new(
                        cUser,
                        FileSystemRights.DeleteSubdirectoriesAndFiles,
                        inherits,
                        PropagationFlags.None,
                        AccessControlType.Allow
                    );
                if (
                    dirSecurity.ModifyAccessRule(AccessControlModification.Add, ReadRule, out _)
                    && dirSecurity.ModifyAccessRule(AccessControlModification.Add, WriteRule, out _)
                    && dirSecurity.ModifyAccessRule(
                        AccessControlModification.Add,
                        ModifyRule,
                        out _
                    )
                    && dirSecurity.ModifyAccessRule(
                        AccessControlModification.Add,
                        ListDirectoryRule,
                        out _
                    )
                    && dirSecurity.ModifyAccessRule(
                        AccessControlModification.Add,
                        CreateFilesRule,
                        out _
                    )
                    && dirSecurity.ModifyAccessRule(
                        AccessControlModification.Add,
                        CreateDirsRule,
                        out _
                    )
                    && dirSecurity.ModifyAccessRule(
                        AccessControlModification.Add,
                        DeleteRule,
                        out _
                    )
                    && dirSecurity.ModifyAccessRule(
                        AccessControlModification.Add,
                        DeleteSubdirectoriesAndFilesRule,
                        out _
                    )
                ) { }
                else
                {
                    throw new Exception("AddAccessRule failed. 你需要手动处理发布内容！");
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
                    WindowsIdentity.GetCurrent().User
                    ?? throw new Exception("GetWindowsIdentity failed. 你需要手动处理发布内容！");
                FileSystemAccessRule ReadRule =
                    new(cUser, FileSystemRights.Read, AccessControlType.Allow);
                FileSystemAccessRule WriteRule =
                    new(cUser, FileSystemRights.Write, AccessControlType.Allow);
                FileSystemAccessRule DeleteRule =
                    new(cUser, FileSystemRights.Delete, AccessControlType.Allow);
                FileSystemAccessRule ExecuteRule =
                    new(cUser, FileSystemRights.ExecuteFile, AccessControlType.Allow);
                if (
                    fileSecurity.ModifyAccessRule(AccessControlModification.Add, ReadRule, out _)
                    && fileSecurity.ModifyAccessRule(
                        AccessControlModification.Add,
                        WriteRule,
                        out _
                    )
                    && fileSecurity.ModifyAccessRule(
                        AccessControlModification.Add,
                        DeleteRule,
                        out _
                    )
                    && fileSecurity.ModifyAccessRule(
                        AccessControlModification.Add,
                        ExecuteRule,
                        out _
                    )
                ) { }
                else
                {
                    throw new Exception("AddAccessRule failed. 你需要手动处理发布内容！");
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
                .GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount))
                .Cast<FileSystemAccessRule>();
#pragma warning disable CA1416 // Validate platform compatibility
            var it =
                from i in ac
                where i.IdentityReference == WindowsIdentity.GetCurrent().User
                select i;
#pragma warning restore CA1416 // Validate platform compatibility
            List<DirAcess> caccess = [];
            foreach (var i in it)
            {
                if (i.FileSystemRights == FileSystemRights.FullControl)
                {
                    return true;
                }
                else if (i.FileSystemRights == FileSystemRights.Read)
                {
                    caccess.Add(DirAcess.Read);
                }
                else if (i.FileSystemRights == FileSystemRights.Write)
                {
                    caccess.Add(DirAcess.Write);
                }
                else if (i.FileSystemRights == FileSystemRights.Delete)
                {
                    caccess.Add(DirAcess.Delete);
                }
                else if (i.FileSystemRights == FileSystemRights.Modify)
                {
                    caccess.Add(DirAcess.Modify);
                }
                else if (i.FileSystemRights == FileSystemRights.ListDirectory)
                {
                    caccess.Add(DirAcess.ListDirectory);
                }
                else if (i.FileSystemRights == FileSystemRights.CreateFiles)
                {
                    caccess.Add(DirAcess.CreateFiles);
                }
                else if (i.FileSystemRights == FileSystemRights.CreateDirectories)
                {
                    caccess.Add(DirAcess.CreateDirectories);
                }
                else if (i.FileSystemRights == FileSystemRights.DeleteSubdirectoriesAndFiles)
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
                .GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount))
                .Cast<FileSystemAccessRule>();
#pragma warning disable CA1416 // Validate platform compatibility
            var it =
                from i in ac
                where i.IdentityReference == WindowsIdentity.GetCurrent().User
                select i;
#pragma warning restore CA1416 // Validate platform compatibility


            List<FileAccess> caccess = [];
            foreach (var i in it)
            {
                if (i.FileSystemRights == FileSystemRights.FullControl)
                {
                    return true;
                }
                else if (i.FileSystemRights == FileSystemRights.Read)
                {
                    caccess.Add(FileAccess.Read);
                }
                else if (i.FileSystemRights == FileSystemRights.Write)
                {
                    caccess.Add(FileAccess.Write);
                }
                else if (i.FileSystemRights == FileSystemRights.Delete)
                {
                    caccess.Add(FileAccess.Delete);
                }
                else if (i.FileSystemRights == FileSystemRights.ExecuteFile)
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
