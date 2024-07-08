using System.IO;

namespace Common;

public enum DirOrFile
{
    Dir = 'd',
    File = 'f',
}

public enum NextOpType
{
    Add = 'a',
    Modify = 'm',
    Del = 'd'
}

public abstract class AFileOrDir(string path, DirOrFile type = DirOrFile.File)
{
    public DirOrFile Type { get; set; } = type;
    public NextOpType? NextOp { get; set; }

    /// <summary>
    /// 全部为绝对路径... 占用资源会大一点，但是完全OK
    /// </summary>
    public string Path { get; set; } = path;
    public abstract bool IsEqual(AFileOrDir other);

    public static int Compare(AFileOrDir l, AFileOrDir r)
    {
        return l.Path.CompareTo(r.Path);
    }
}

public class File(string path, DateTime mtime) : AFileOrDir(path)
{
    public DateTime MTime { get; set; } = mtime;

    public override bool IsEqual(AFileOrDir other)
    {
        if (other is not File otherFile)
        {
            return false;
        }
        else
        {
            return this.MTime == otherFile.MTime && this.Path == otherFile.Path;
        }
    }
}

public class Dir(string path) : AFileOrDir(path, DirOrFile.Dir)
{
    public List<AFileOrDir> Children { get; set; } = [];

    public override bool IsEqual(AFileOrDir other)
    {
        if (other is not Dir otherDir)
        {
            return false;
        }
        else
        {
            if (this.Path != otherDir.Path)
            {
                return false;
            }
            if (this.Children.Count != otherDir.Children.Count)
            {
                return false;
            }
            this.Children.Sort(AFileOrDir.Compare);
            otherDir.Children.Sort(AFileOrDir.Compare);
            for (int i = 0; i < this.Children.Count; i++)
            {
                if (!this.Children[i].IsEqual(otherDir.Children[i]))
                {
                    return false;
                }
            }
            return true;
        }
    }

    public void ExtractInfo()
    {
        string[] files = Directory.GetFiles(this.Path);
        string[] dirs = Directory.GetDirectories(this.Path);
        foreach (var file in files)
        {
            this.Children.Add(new File(file, System.IO.File.GetLastWriteTime($"{file}")));
        }
        foreach (var dir in dirs)
        {
            var ndir = new Dir(dir);
            ndir.ExtractInfo();
            this.Children.Add(ndir);
        }
    }
}

public class RootDir(string path) : Dir(path)
{
    public override bool IsEqual(AFileOrDir other)
    {
        if (other is not RootDir otherDir)
        {
            return false;
        }
        if (this.Path != otherDir.Path)
        {
            return false;
        }
        else
        {
            return base.IsEqual(otherDir);
        }
    }

#pragma warning disable CA2211 // Non-constant fields should not be visible
    public static Func<File, (bool, string)> WriteFileStrageFunc = (File file) =>
    {
        return (false, "you must implement this function!");
    };
#pragma warning restore CA2211 // Non-constant fields should not be visible
#pragma warning disable CA2211 // Non-constant fields should not be visible
    public static Func<string, (bool, string)> WriteDirStrageFunc = (string path) =>
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        return (true, "");
    };
#pragma warning restore CA2211 // Non-constant fields should not be visible
    /// <summary>
    /// 只比较内容是否相同
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool IsContentEqual(AFileOrDir other)
    {
        if (other is not RootDir otherDir)
        {
            return false;
        }
        else
        {
            return base.IsEqual(otherDir);
        }
    }

    public (bool, string) WriteByThisInfo()
    {
        static (bool, string) f(Dir dir)
        {
            foreach (var child in dir.Children)
            {
                Console.WriteLine(child.Path);
                if (child.Type == DirOrFile.Dir)
                {
                    var (IsSuccess, Message) = WriteDirStrageFunc(child.Path);
                    if (!IsSuccess)
                    {
                        return (false, Message);
                    }
                    if (child is Dir childDir)
                    {
                        f(childDir);
                    }
                }
                else
                {
                    if (child is File childFile)
                    {
                        var (IsSuccess, Message) = WriteFileStrageFunc(childFile);
                        if (!IsSuccess)
                        {
                            return (false, Message);
                        }
                    }
                    else
                    {
                        return (false, "child is not File!");
                    }
                }
            }
            return (true, "");
        }
        return f(this);
    }

    public new (bool, string) ExtractInfo()
    {
        if (this.Children.Count != 0)
        {
            return (false, "this RootDir is not empty.");
        }
        string[] files = Directory.GetFiles(this.Path);
        string[] dirs = Directory.GetDirectories(this.Path);
        foreach (var file in files)
        {
            this.Children.Add(new File(file, System.IO.File.GetLastWriteTime($"{file}")));
        }
        foreach (var dir in dirs)
        {
            var ndir = new Dir(dir);
            ndir.ExtractInfo();
            this.Children.Add(ndir);
        }
        return (true, "");
    }
}
