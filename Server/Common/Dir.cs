namespace Common;

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

    public Dir Clone(NextOpType? optype, string oldRootPath, string newRootPath, bool IsResetNextOpType = false)
    {
        var ndir = this.Clone(optype, IsResetNextOpType);
        ndir.ResetRootPath(oldRootPath, newRootPath);
        return ndir;
    }

    public Dir Clone(NextOpType? optype = null, bool IsResetNextOpType = false)
    {
        var ndir = new Dir(this.Path, [], IsResetNextOpType ? optype : this.NextOp);

        var nchildren = this
            .Children.AsEnumerable()
            .Select(x =>
            {
                if (x is File file)
                {
                    return new File(file.Path, file.MTime, IsResetNextOpType ? optype : file.NextOp) as AFileOrDir;
                }
                else if (x is Dir dir)
                {
                    return dir.Clone(optype, IsResetNextOpType);
                }
                else
                {
                    throw new Exception("cannot be here!");
                }
            })
            .ToList();

        return ndir;
    }
    public void ResetRootPath(string oldPath, string newPath)
    {
        this.Path = this.Path.Replace(oldPath, newPath);
        this.Children.ForEach(e =>
        {
            if (e is File file)
            {
                file.Path = file.Path.Replace(oldPath, newPath);
            }
            else if (e is Dir dir)
            {
                dir.ResetRootPath(oldPath, newPath);
            }
        });

    }

    /// <summary>
    /// 合并两个文件夹,other不会发生改变，this将合并一个副本
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
            var ldir = this;
            var rdir = other;


            foreach (var oc in other.Children)
            {
                if (oc is File rfile)
                {
                    if (rfile.NextOp != null)
                    {
                        if (oc.NextOp == NextOpType.Add)
                        {
                            ldir.AddChild(new File(rfile.Path, rfile.MTime, rfile.NextOp));
                        }
                        else
                        {
                            var n = ldir.Children.Where(x => x.Path == oc.Path && x.Type == DirOrFile.File).FirstOrDefault();
                            if (n is not null)
                            {
                                if (oc.NextOp == NextOpType.Del)
                                {
                                    ldir.Children.Remove(n);
                                }
                                else if (oc.NextOp == NextOpType.Modify)
                                {
                                    if (n is File lfile)
                                    {
                                        lfile.MTime = rfile.MTime;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (oc is Dir rrdir)
                {
                    //新增和删除意味着整个文件夹都被新增和删除
                    if (rrdir.NextOp == NextOpType.Add)
                    {
                        ldir.AddChild(rrdir.Clone(null, true));
                    }
                    else if (rrdir.NextOp == NextOpType.Del)
                    {
                        ldir.Children.Remove(oc);
                    }
                    //当子文件夹和文件不确定时
                    else
                    {
                        var n = ldir.Children.Where(x => x.Path == rrdir.Path && x.Type == DirOrFile.Dir).FirstOrDefault();
                        if (n is Dir lldir)
                        {
                            lldir.Combine(rrdir);

                        }
                    }
                }
            }
        }
        return (true, "");
    }

    /// <summary>
    /// 添加子节点,根目录相同,才会被添加进去
    /// </summary>
    /// <param name="child"></param>
    /// <returns></returns>/
    protected (bool, string) AddChild(AFileOrDir child)
    {
        if (child.Path.Substring(0, this.Path.Length) != this.Path)
        {
            return (false, "their rootpath are not same!");
        }
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
                        ndir.AddChild(d);
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
    /// <summary>
    /// 从文件目录结构提起文件信息，注意，此目录文件树不包含文件内容，仅有修改时间mtime
    /// </summary>
    /// <returns></returns>
    public (bool, string) ExtractInfo()
    {
        if (this.Children.Count != 0)
        {
            return (false, "this dir is not empty.");
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
    /// <summary>
    ///  写入目录文件树，首先必须定义写入文件的策略，此目录结构不包含文件内容,但有一个
    ///  文件的修改时间，是否修改文件的修改时间，需要定义文件的写入策略 WriteFileStrageFunc
    /// </summary>
    /// <returns></returns>
    public (bool, string) WriteByThisInfo()
    {
        static (bool, string) f(Dir dir)
        {
            foreach (var child in dir.Children)
            {
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
    /// 比较两个目录文件树是否相同,不相同返回差异部分,左侧是右侧的下一个版本
    /// </summary>
    /// <param name="otherRootDir"></param>
    /// <returns></returns>
    public (bool, Dir?) Diff(Dir other)
    {
        var ldir = this;
        var rdir = other;
        Dir? cDir = new Dir(rdir.Path);
        //分别对文件和文件夹分组
        List<File> lFiles = [];
        List<File> rFiles = [];
        List<Dir> lDirs = [];
        List<Dir> rDirs = [];
        var lGroups = ldir.Children.GroupBy(x => x.Type);
        var rGroups = rdir.Children.GroupBy(x => x.Type);
        foreach (var g in lGroups)
        {
            if (g.Key == DirOrFile.Dir)
            {
                lDirs = g.AsEnumerable()
                    .Select(n =>
                    {
                        if (n is Dir dir)
                        {
                            return dir;
                        }
                        throw new Exception("cannot be here");
                    })
                    .ToList();
            }
            else
            {
                lFiles = g.AsEnumerable()
                    .Select(n =>
                    {
                        if (n is File file)
                        {
                            return file;
                        }
                        throw new Exception("cannot be here");
                    })
                    .ToList();
            }
        }

        foreach (var g in rGroups)
        {
            if (g.Key == DirOrFile.Dir)
            {
                rDirs = g.AsEnumerable()
                    .Select(n =>
                    {
                        if (n is Dir dir)
                        {
                            return dir;
                        }
                        throw new Exception("cannot be here");
                    })
                    .ToList();
            }
            else
            {
                rFiles = g.AsEnumerable()
                    .Select(n =>
                    {
                        if (n is File file)
                        {
                            return file;
                        }
                        throw new Exception("cannot be here");
                    })
                    .ToList();
            }
        }


        //排序，然后对比
        int lIndex_f = 0;
        int rIndex_f = 0;
        int lIndex_d = 0;
        int rIndex_d = 0;
        lFiles.Sort(Compare);
        rFiles.Sort(Compare);
        lDirs.Sort(Compare);
        rDirs.Sort(Compare);
        //对比文件
        while (true)
        {
            //当两个线性表都走到最后时，退出循环
            if (lIndex_f == lFiles.Count && rIndex_f == rFiles.Count)
            {
                break;
            }
            //左侧先到底，右侧都是将要删除的
            if (lIndex_f == lFiles.Count)
            {
                var er = rFiles[rIndex_f];
                cDir.Children.Add(new File(er.Path, er.MTime, NextOpType.Del));
                rIndex_f++;
                continue;
            }
            //右侧先到底，左侧都是要添加的
            if (rIndex_f == rFiles.Count)
            {
                var el = lFiles[lIndex_f];

                cDir.Children.Add(
                    new File(el.Path.Replace(ldir.Path, rdir.Path), el.MTime, NextOpType.Add)
                );
                lIndex_f++;
                continue;
            }
            var l = lFiles[lIndex_f];
            var r = rFiles[rIndex_f];
            //将根路径差异抹平
            var lreativePath = l.Path.Replace(ldir.Path, "");
            var rreativePath = r.Path.Replace(rdir.Path, "");
            //两文件相同，对比文件修改时间，不同增加到diff内容
            if (lreativePath == rreativePath)
            {
                lIndex_f++;
                rIndex_f++;
                if (l.MTime != r.MTime)
                {
                    cDir.Children.Add(new File(r.Path, l.MTime, NextOpType.Modify));
                }
            }
            else
            {
                //因为已经按照文件路径排过序了，当左侧文件名大于右侧，那么将根据右侧,添加一个删除diff
                if (lreativePath.CompareTo(rreativePath) > 0)
                {
                    rIndex_f++;
                    cDir.Children.Add(new File(r.Path, r.MTime, NextOpType.Del));
                }
                //相反，根据左侧，添加一个新增diff
                else
                {
                    lIndex_f++;
                    cDir.Children.Add(
                        new File(l.Path.Replace(ldir.Path, rdir.Path), l.MTime, NextOpType.Add)
                    );
                }
            }
        }

        //文件夹的比较和文件类似，但是他会递归调用文件夹的diff函数，直至文件停止

        while (true)
        {
            if (lIndex_d == lDirs.Count && rIndex_d == rDirs.Count)
            {
                break;
            }
            if (lIndex_d == lDirs.Count)
            {
                var er = rDirs[rIndex_d];
                rIndex_d++;
                continue;
            }
            if (rIndex_d == rDirs.Count)
            {
                var el = lDirs[lIndex_d];
                lIndex_d++;
                continue;
            }
            var l = lDirs[lIndex_d];
            var r = rDirs[rIndex_d];
            var lreativePath = l.Path.Replace(ldir.Path, "");
            var rreativePath = r.Path.Replace(rdir.Path, "");
            if (lreativePath == rreativePath)
            {
                lIndex_d++;
                rIndex_d++;
                var (IsSuccess, rDir) = l.Diff(r);
                if (IsSuccess && rDir is not null)
                {
                    //而等于0，这表示此文件夹的内容没有变化
                    if (rDir.Children.Count != 0)
                    {
                        cDir.Children.Add(rDir);
                    }
                }
            }
            else
            {
                //文件夹重命名将会触发整个文件夹的删除和新增操作，这里没有办法定位到操作是修改文件夹(?) 和git类似。
                //潜在的问题是，修改文件夹名，此文件夹包含大量的文件，将触发大量操作。

                if (lreativePath.CompareTo(rreativePath) > 0)
                {
                    cDir.Children.Add(r.Clone(NextOpType.Del, true));
                    rIndex_d++;
                }
                else
                {
                    cDir.Children.Add(l.Clone(NextOpType.Add, ldir.Path, rdir.Path, true));
                    lIndex_d++;
                }
            }
        }
        if (cDir.Children.Count == 0)
        {
            return (false, null);
        }
        else
        {
            return (true, cDir);
        }
    }
}
