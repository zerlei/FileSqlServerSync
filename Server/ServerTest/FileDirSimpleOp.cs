using System.Text;
using Common;

namespace ServerTest;

/// <summary>
/// 简单文件操作
/// </summary>
public class SimpleFileDirOp : FileDirOpStra
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
        if (System.IO.File.Exists(absolutePath))
        {
            System.IO.File.Delete(absolutePath);
        }
    }

    public override void DirCreate(Dir dir, bool IsRecursion = true)
    {
        if (!Directory.Exists(dir.FormatedPath))
        {
            Directory.CreateDirectory(dir.FormatedPath);
            if (IsRecursion)
            {
                foreach (var fd in dir.Children)
                {
                    if (fd is Common.File file)
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
