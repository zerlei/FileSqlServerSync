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

public abstract class AFileOrDir(
    string path,
    DirOrFile type = DirOrFile.File,
    NextOpType? nextOp = null
)
{
    public DirOrFile Type { get; set; } = type;
    public NextOpType? NextOp { get; set; } = nextOp;

    /// <summary>
    /// 全部为绝对路径... 占用资源会大一点，但是完全OK
    /// </summary>
    public string Path { get; set; } = path;
    public abstract bool IsEqual(AFileOrDir other);

    public static int Compare(AFileOrDir l, AFileOrDir r)
    {
        var pv = l.Path.CompareTo(r.Path);
        if (pv == 0)
        {
            pv = l.Type.CompareTo(r.Type);
        }
        return pv;
    }
}

/// <summary>
/// 文件
/// </summary>
/// <param name="path">绝对路径</param>
/// <param name="mtime">文件的修改时间</param>/
public class File(string path, DateTime mtime, NextOpType? nextOp = null)
    : AFileOrDir(path, DirOrFile.File, nextOp)
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
            return this.MTime == otherFile.MTime
                && this.Path == otherFile.Path
                && this.NextOp == otherFile.NextOp;
        }
    }
}

/// <summary>
/// 文件夹结构，它包含文件和文件夹
/// </summary>
/// <param name="path">绝对路径</param>
/// <param name="children">子文件或文件夹</param>
public class Dir(string path, List<AFileOrDir>? children = null, NextOpType? nextOp = null)
    : AFileOrDir(path, DirOrFile.Dir, nextOp)
{
    public List<AFileOrDir> Children { get; set; } = children ?? [];

    public override bool IsEqual(AFileOrDir other)
    {
        if (other is not Dir otherDir)
        {
            return false;
        }
        else
        {
            if (this.Path != otherDir.Path || this.NextOp != otherDir.NextOp)
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

    public Dir Clone(NextOpType? optype, string oldRootPath, string newRootPath)
    {
        var ndir = new Dir(this.Path.Replace(oldRootPath, newRootPath));

        var nchildren = this
            .Children.AsEnumerable()
            .Select(x =>
            {
                if (x is File file)
                {
                    return new File(
                            file.Path.Replace(oldRootPath, newRootPath),
                            file.MTime,
                            file.NextOp
                        ) as AFileOrDir;
                }
                else if (x is Dir dir)
                {
                    return dir.Clone(optype, oldRootPath, newRootPath);
                }
                else
                {
                    throw new Exception("cannot be here!");
                }
            })
            .ToList();

        return ndir;
    }

    public Dir Clone(NextOpType? optype)
    {
        var ndir = new Dir(this.Path);

        var nchildren = this
            .Children.AsEnumerable()
            .Select(x =>
            {
                if (x is File file)
                {
                    return new File(file.Path, file.MTime, file.NextOp) as AFileOrDir;
                }
                else if (x is Dir dir)
                {
                    return dir.Clone(optype);
                }
                else
                {
                    throw new Exception("cannot be here!");
                }
            })
            .ToList();

        return ndir;
    }

    /// <summary>
    /// 合并两个文件夹
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public (bool, string) Combine(Dir other)
    {
        if (this.Path != other.Path)
        {
            return (false, "their path is not same");
        }
        else
        {
            foreach (var oc in other.Children)
            {
                var (IsSuccess, Message) = this.AddChild(oc);
                if (!IsSuccess)
                {
                    return (IsSuccess, Message);
                }
            }
        }
        return (true, "");
    }

    /// <summary>
    /// 添加子节点 但是注意，这里没有判断根节点路径是否相同，它们在相同的父目录下
    /// </summary>
    /// <param name="child"></param>
    /// <returns></returns>/
    protected (bool, string) AddChild(AFileOrDir child)
    {
        var filtedChildren = this.Children.Where(x => x.Type == child.Type);

        var mached = filtedChildren.Where(x => x.Path == child.Path);

        if (mached.Any())
        {
            if (child is File)
            {
                return (false, "there are same path in the children");
            }
            else if (child is Dir dir)
            {
                var tdir = mached.FirstOrDefault();
                if (tdir is Dir ndir)
                {
                    foreach (var d in dir.Children)
                    {
                        var (IsSuccess, Message) = ndir.AddChild(d);
                        if (!IsSuccess)
                        {
                            return (IsSuccess, Message);
                        }
                    }
                }
            }
        }
        else
        {
            this.Children.Add(child);
        }
        return (true, "");
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


