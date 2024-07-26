using System.Text;

namespace Common;

public abstract class FileDirOp
{
    public abstract void FileCreate(string absolutePath, DateTime mtime);

    public abstract void DirCreate(Dir dir, bool IsRecursion = true);

    public abstract void FileModify(string absolutePath, DateTime mtime);
    public abstract void FileDel(string absolutePath);
    public abstract void DirDel(Dir dir, bool IsRecursion = true);
}

public class SimpleFileDirOpForTest : FileDirOp
{
    public override void FileCreate(string absolutePath, DateTime mtime)
    {
        using (FileStream fs = System.IO.File.OpenWrite(absolutePath))
        {
            byte[] info = Encoding.UTF8.GetBytes($"this is  {absolutePath},Now{mtime}");
            fs.Write(info, 0, info.Length);
        }
        System.IO.File.SetLastWriteTime(absolutePath, mtime);
    }

    public override void FileModify(string absolutePath, DateTime mtime)
    {
        using (FileStream fs = System.IO.File.OpenWrite(absolutePath))
        {
            byte[] info = Encoding.UTF8.GetBytes($"this is  {absolutePath},Now{mtime}");
            fs.Write(info, 0, info.Length);
        }
        System.IO.File.SetLastWriteTime(absolutePath, mtime);
    }

    public override void FileDel(string absolutePath)
    {
        //ToDo 权限检查
        if (System.IO.File.Exists(absolutePath))
        {
            System.IO.File.Delete(absolutePath);
        }
    }

    public override void DirCreate(Dir dir, bool IsRecursion = true)
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
    }

    public override void DirDel(Dir dir, bool IsRecursion = true)
    {
        //TODO 权限检查 正式徐执行递归
        if (!IsRecursion)
        {
            if (Directory.Exists(dir.FormatedPath))
            {
                Directory.Delete(dir.FormatedPath, true);
            }
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}
