using System.Text;

namespace Common;

public abstract class FileDirOp
{
    public abstract (bool, string) FileCreate(string absolutePath, DateTime mtime);

    public abstract (bool, string) DirCreate(Dir dir, bool IsRecursion = true);

    public abstract (bool, string) FileModify(string absolutePath, DateTime mtime);
    public abstract (bool, string) FileDel(string absolutePath);
    public abstract (bool, string) DirDel(Dir dir, bool IsRecursion = true);
}

public class SimpleFileDirOpForTest : FileDirOp
{
    public override (bool, string) FileCreate(string absolutePath, DateTime mtime)
    {
        using (FileStream fs = System.IO.File.OpenWrite(absolutePath))
        {
            byte[] info = Encoding.UTF8.GetBytes($"this is  {absolutePath},Now{mtime}");
            fs.Write(info, 0, info.Length);
        }
        System.IO.File.SetLastWriteTime(absolutePath, mtime);
        return (true, "");
    }

    public override (bool, string) FileModify(string absolutePath, DateTime mtime)
    {
        using (FileStream fs = System.IO.File.OpenWrite(absolutePath))
        {
            byte[] info = Encoding.UTF8.GetBytes($"this is  {absolutePath},Now{mtime}");
            fs.Write(info, 0, info.Length);
        }
        System.IO.File.SetLastWriteTime(absolutePath, mtime);
        return (true, "");
    }

    public override (bool, string) FileDel(string absolutePath)
    {
        //ToDo 权限检查
        if (System.IO.File.Exists(absolutePath))
        {
            System.IO.File.Delete(absolutePath);
        }
        return (true, "");
    }

    public override (bool, string) DirCreate(Dir dir, bool IsRecursion = true)
    {
        //TODO需做权限检查
        if (!Directory.Exists(dir.FormatedPath))
        {
            Directory.CreateDirectory(dir.FormatedPath);
            if (IsRecursion)
            {
                foreach (var fd in dir.Children)
                {
                    if (fd is File file)
                    {
                        this.FileCreate(file.FormatedPath, file.MTime);
                    }
                    else if (fd is Dir sdir)
                    {
                        DirCreate(sdir);
                    }
                }
            }
        }
        return (true, "");
    }

    public override (bool, string) DirDel(Dir dir, bool IsRecursion = true)
    {
        //TODO 权限检查 正式徐执行递归
        if (!IsRecursion)
        {
            if (Directory.Exists(dir.FormatedPath))
            {
                Directory.Delete(dir.FormatedPath, true);
            }
            return (true, "");
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}
