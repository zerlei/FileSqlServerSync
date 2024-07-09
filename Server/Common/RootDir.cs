namespace Common;

/// <summary>
/// 根目录,最上层父节点
/// </summary>
/// <param name="path">绝对路径</param>
/// <param name="children">子文件或文件夹</param>
public class RootDir(string path, List<AFileOrDir>? children = null) : Dir(path, children)
{
    public override bool IsEqual(AFileOrDir other)
    {
        if (other is not RootDir otherDir)
        {
            return false;
        }
        return this.IsContentEqual(otherDir);
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

    /// <summary>
    /// 从文件目录结构提起文件信息，注意，此目录文件树不包含文件内容，仅有修改时间mtime
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// 比较两个目录文件树是否相同,不相同返回差异部分,左侧是右侧的下一个版本
    /// </summary>
    /// <param name="otherRootDir"></param>
    /// <returns></returns>
    public (bool, RootDir?) Diff(RootDir otherRootDir)
    {
        static Dir? f(Dir ldir, Dir rdir)
        {
            ldir.Children.Sort(AFileOrDir.Compare);
            rdir.Children.Sort(AFileOrDir.Compare);
            Dir? cDir = null;
            if (rdir is RootDir rootdir)
            {
                cDir = new RootDir(rootdir.Path);
            }
            else
            {
                cDir = new Dir(rdir.Path);
            }

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
                            //不可能在这一步
                            return new Dir("");
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
                            return new File("", DateTime.Now);
                        })
                        .ToList();
                }
            }

            int lIndex_f = 0;
            int rIndex_f = 0;
            int lIndex_d = 0;
            int rIndex_d = 0;
            while (true)
            {
                if (lIndex_f == lFiles.Count && rIndex_f == rFiles.Count)
                {
                    break;
                }
                if (lIndex_f == lFiles.Count)
                {
                    var er = rFiles[rIndex_f];
                    cDir.Children.Add(new File(er.Path, er.MTime, NextOpType.Del));
                    rIndex_f++;
                    continue;
                }
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
                var lreativePath = l.Path.Replace(ldir.Path, "");
                var rreativePath = r.Path.Replace(rdir.Path, "");
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
                    if (lreativePath.CompareTo(rreativePath) > 0)
                    {
                        rIndex_f++;
                        cDir.Children.Add(new File(r.Path, r.MTime, NextOpType.Del));
                    }
                    else
                    {
                        lIndex_f++;
                        cDir.Children.Add(
                            new File(l.Path.Replace(ldir.Path, rdir.Path), l.MTime, NextOpType.Add)
                        );
                    }
                }
            }

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
                    var result = f(l, r);
                    if (result is not null)
                    {
                        if (result.Children.Count != 0)
                        {
                            cDir.Children.Add(result);
                        }
                    }
                }
                else
                {
                    if (lreativePath.CompareTo(rreativePath) > 0)
                    {
                        cDir.Children.Add(r.Clone(NextOpType.Del));
                        rIndex_d++;
                    }
                    else
                    {
                        cDir.Children.Add(l.Clone(NextOpType.Add, ldir.Path, rdir.Path));
                        lIndex_d++;
                    }
                }
            }
            if (cDir.Children.Count == 0)
            {
                return null;
            }
            else
            {
                return cDir;
            }
        }

        var diffDir = f(this, otherRootDir);
        if (diffDir is RootDir rootDir)
        {
            if (rootDir.Children.Count == 0)
            {
                return (false, null);
            }
            return (true, rootDir);
        }
        return (false, null);
    }

    /// <summary>
    /// 合并两个文件树,可以合并时(根节点路径相同),返回一个新的根文件树,若存在相同的文件会报错
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>/
    public (bool, string) Combine(RootDir other)
    {
        return base.Combine(other);
    }
}
