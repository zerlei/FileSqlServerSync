namespace Common;

/// <summary>
/// Dir比较方法
/// </summary>
public static class DirExtension
{
    /// <summary>
    /// 比较两个目录文件树是否相同,不相同返回差异部分,左侧是右侧的下一个版本,任何一个节点的nextop != null，即所有
    /// 节点都会打上标记
    /// 文件夹的 NextOp 只有新增和删除
    ///
    /// </summary>
    /// <param name="otherRootDir"></param>
    /// <returns>右侧版本接下来进行的操作</returns>
    public static Dir Diff(this Dir thisDir, Dir other)
    {
        var ldir = thisDir;
        var rdir = other;
        Dir? cDir = new() { Path = rdir.FormatedPath, Children = [] };
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
        lFiles.Sort(AFileOrDir.Compare);
        rFiles.Sort(AFileOrDir.Compare);
        lDirs.Sort(AFileOrDir.Compare);
        rDirs.Sort(AFileOrDir.Compare);
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
                cDir.Children.Add(
                    new File
                    {
                        Path = er.FormatedPath,
                        MTime = er.MTime,
                        NextOp = NextOpType.Del
                    }
                );
                rIndex_f++;
                continue;
            }
            //右侧先到底，左侧都是要添加的
            if (rIndex_f == rFiles.Count)
            {
                var el = lFiles[lIndex_f];

                cDir.Children.Add(
                    new File
                    {
                        Path = el.FormatedPath.Replace(ldir.FormatedPath, rdir.FormatedPath),
                        MTime = el.MTime,
                        NextOp = NextOpType.Add
                    }
                );
                lIndex_f++;
                continue;
            }
            var l = lFiles[lIndex_f];
            var r = rFiles[rIndex_f];
            //将根路径差异抹平
            var lreativePath = l.FormatedPath.Replace(ldir.FormatedPath, "");
            var rreativePath = r.FormatedPath.Replace(rdir.FormatedPath, "");
            //两文件相同，对比文件修改时间，不同增加到diff内容
            if (lreativePath == rreativePath)
            {
                lIndex_f++;
                rIndex_f++;
                // 如果最新版的时间大于旧版时间超过5s才更新，文件时间在传输过程中产生了精度损失。
                // Warrning 如果旧版文件的最后修改时间大于新版文件，将不会更新
                if ((l.MTime - r.MTime).TotalSeconds > 5)
                {
                    cDir.Children.Add(
                        new File
                        {
                            Path = r.FormatedPath,
                            MTime = l.MTime,
                            NextOp = NextOpType.Modify
                        }
                    );
                }
            }
            else
            {
                //因为已经按照文件路径排过序了，当左侧文件名大于右侧，那么将根据右侧,添加一个删除diff
                if (lreativePath.CompareTo(rreativePath) > 0)
                {
                    rIndex_f++;
                    cDir.Children.Add(
                        new File
                        {
                            Path = r.FormatedPath,
                            MTime = r.MTime,
                            NextOp = NextOpType.Del
                        }
                    );
                }
                //相反，根据左侧，添加一个新增diff
                else
                {
                    lIndex_f++;
                    cDir.Children.Add(
                        new File
                        {
                            Path = l.FormatedPath.Replace(ldir.FormatedPath, rdir.FormatedPath),
                            MTime = l.MTime,
                            NextOp = NextOpType.Add
                        }
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
                cDir.Children.Add(er.Clone(NextOpType.Del, true));
                rIndex_d++;
                continue;
            }
            if (rIndex_d == rDirs.Count)
            {
                var el = lDirs[lIndex_d];
                cDir.Children.Add(
                    el.Clone(NextOpType.Add, true)
                        .ResetRootPath(ldir.FormatedPath, rdir.FormatedPath)
                );
                lIndex_d++;
                continue;
            }
            var l = lDirs[lIndex_d];
            var r = rDirs[rIndex_d];
            var lreativePath = l.FormatedPath.Replace(ldir.FormatedPath, "");
            var rreativePath = r.FormatedPath.Replace(rdir.FormatedPath, "");
            if (lreativePath == rreativePath)
            {
                lIndex_d++;
                rIndex_d++;
                var rDir = l.Diff(r);
                //而等于0，这表示此文件夹的内容没有变化
                if (rDir.Children.Count != 0)
                {
                    cDir.Children.Add(rDir);
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
                    cDir.Children.Add(
                        l.Clone(NextOpType.Add, true)
                            .ResetRootPath(ldir.FormatedPath, rdir.FormatedPath)
                    );
                    lIndex_d++;
                }
            }
        }
        return cDir;
    }


    /// <summary>
    /// 根据Dirobject记录的信息，写入磁盘
    /// </summary>
    /// <param name="thisDir"></param>
    /// <param name="fileDirOp"> 文件操作类,它是如何写入文件的方法</param>
    /// <exception cref="ArgumentException"></exception>
    public static void WriteByThisInfo(this Dir thisDir, FileDirOpStra fileDirOp)
    {
        static void f(Dir dir, FileDirOpStra fileDirOp)
        {
            dir.AccessCheck();
            foreach (var child in dir.Children)
            {
                if (child.Type == DirOrFile.Dir)
                {
                    if (child is Dir childDir)
                    {
                        if (childDir.NextOp != NextOpType.Del)
                        {
                            fileDirOp.DirCreate(childDir, false);
                            f(childDir, fileDirOp);
                        }
                    }
                }
                else
                {
                    if (child is File childFile)
                    {
                        if (childFile.NextOp != NextOpType.Del)
                        {
                            fileDirOp.FileCreate(child.FormatedPath, childFile.MTime);
                        }
                    }
                    else
                    {
                        throw new ArgumentException("child is not File!");
                    }
                }
            }
        }
        f(thisDir, fileDirOp);
    }

    /// <summary>
    ///  从文件夹中提取文件信息
    /// </summary>
    /// <param name="thisDir"></param>
    /// <param name="cherryPicks">只提取这些信息，最高级别</param>
    /// <param name="exculdes">忽略这些文件信息</param>
    /// <exception cref="NotSupportedException"></exception>
    public static void ExtractInfo(
        this Dir thisDir,
        List<string>? cherryPicks = null,
        List<string>? exculdes = null
    )
    {
        bool filter(string path)
        {
            var relativePath = path.Replace('\\', '/').Replace(thisDir.FormatedPath, "");
            if (cherryPicks != null)
            {
                return cherryPicks.Contains(relativePath);
            }

            if (exculdes != null)
            {
                return !exculdes.Contains(relativePath);
            }
            return true;
        }

        if (thisDir.Children.Count != 0)
        {
            throw new NotSupportedException("this dir is not empty.");
        }
        if (Directory.Exists(thisDir.FormatedPath))
        {
            string[] files = Directory.GetFiles(thisDir.FormatedPath);
            string[] dirs = Directory.GetDirectories(thisDir.FormatedPath);
            foreach (var file in files)
            {
                if (filter(file))
                {
                    thisDir.Children.Add(
                        new File { Path = file, MTime = System.IO.File.GetLastWriteTime($"{file}") }
                    );
                }
            }
            foreach (var dir in dirs)
            {
                if (filter(dir))
                {
                    var ndir = new Dir { Path = dir, Children = [] };
                    ndir.ExtractInfo(cherryPicks, exculdes);
                    thisDir.Children.Add(ndir);
                }
            }
        }
    }
    /// <summary>
    /// 添加一个子对象,这个不包含文件或文件夹的创建
    /// </summary>
    /// <param name="thisDir"></param>
    /// <param name="child"></param>
    /// <exception cref="ArgumentException"></exception>
    public static void AddChild(this Dir thisDir, AFileOrDir child)
    {
        if (child.FormatedPath[..thisDir.FormatedPath.Length] != thisDir.FormatedPath)
        {
            throw new ArgumentException("their rootpath are not same!");
        }
        var filtedChildren = thisDir.Children.Where(x => x.Type == child.Type);

        var mached = filtedChildren.Where(x => x.FormatedPath == child.FormatedPath);

        if (mached.Any())
        {
            if (child is File)
            {
                throw new ArgumentException(
                    $"there are same path in the children:{child.FormatedPath}"
                );
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
            thisDir.Children.Add(child);
        }
    }


    /// <summary>
    /// 合并diffdir中的内容
    /// </summary>
    /// <param name="thisDir"></param>
    /// <param name="diffdir"></param>
    /// <param name="IsUpdateObject">是否更新对象</param>
    /// <param name="IsUpdateDirFile">是否更新文件夹和文件</param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="NotImplementedException"></exception>
    public static void Combine(
        this Dir thisDir,
        FileDirOpStra? fileDirOp,
        Dir diffdir,
        bool IsUpdateObject = true,
        bool IsUpdateDirFile = false
    )
    {
        ///验证 target 文件或者文件夹是否具备相关文件权限。
        if (IsUpdateDirFile)
        {
            diffdir.AccessCheck();
        }
        if (thisDir.FormatedPath != diffdir.FormatedPath)
        {
            throw new ArgumentException("their path is not same");
        }
        else
        {
            var ldir = thisDir;
            var rdir = diffdir;

            foreach (var oc in diffdir.Children)
            {
                if (oc is File rfile)
                {
                    if (rfile.NextOp != null)
                    {
                        if (oc.NextOp == NextOpType.Add)
                        {
                            if (IsUpdateObject)
                            {
                                ldir.AddChild(
                                    new File { Path = rfile.FormatedPath, MTime = rfile.MTime }
                                );
                            }
                            if (IsUpdateDirFile)
                            {
                                fileDirOp?.FileCreate(rfile.FormatedPath, rfile.MTime);
                            }
                        }
                        else
                        {
                            var n = ldir
                                .Children.Where(x =>
                                    x.FormatedPath == oc.FormatedPath && x.Type == DirOrFile.File
                                )
                                .FirstOrDefault();
                            if (n is not null)
                            {
                                if (oc.NextOp == NextOpType.Del)
                                {
                                    if (IsUpdateObject)
                                    {
                                        ldir.Children.Remove(n);
                                    }
                                    if (IsUpdateDirFile)
                                    {
                                        fileDirOp?.FileDel(rfile.FormatedPath);
                                    }
                                }
                                else if (oc.NextOp == NextOpType.Modify)
                                {
                                    if (n is File lfile)
                                    {
                                        if (IsUpdateObject)
                                        {
                                            lfile.MTime = rfile.MTime;
                                        }
                                        if (IsUpdateDirFile)
                                        {
                                            fileDirOp?.FileModify(rfile.FormatedPath, rfile.MTime);
                                        }
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
                        if (IsUpdateDirFile)
                        {
                            fileDirOp?.DirCreate(rrdir);
                        }
                        if (IsUpdateObject)
                        {
                            ldir.AddChild(rrdir.Clone(null, true));
                        }
                    }
                    else if (rrdir.NextOp == NextOpType.Del)
                    {
                        if (IsUpdateDirFile)
                        {
                            fileDirOp?.DirDel(rrdir, false);
                        }
                        if (IsUpdateObject)
                        {
                            ldir.Children.RemoveAt(
                                ldir.Children.FindIndex(x => x.FormatedPath == rrdir.FormatedPath)
                            );
                        }
                    }
                    //当子文件夹和文件不确定时
                    else
                    {
                        var n = ldir
                            .Children.Where(x =>
                                x.FormatedPath == rrdir.FormatedPath && x.Type == DirOrFile.Dir
                            )
                            .FirstOrDefault();
                        if (n is Dir lldir)
                        {
                            lldir.Combine(fileDirOp, rrdir, IsUpdateObject, IsUpdateDirFile);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// clone一个新的对象
    /// </summary>
    /// <param name="thisDir"></param>
    /// <param name="optype">重设的操作类型</param>
    /// <param name="IsResetNextOpType">是否重设操作类型</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static Dir Clone(
        this Dir thisDir,
        NextOpType? optype = null,
        bool IsResetNextOpType = false
    )
    {
        var ndir = new Dir
        {
            Path = thisDir.FormatedPath,
            Children = [],
            NextOp = IsResetNextOpType ? optype : thisDir.NextOp
        };

        var nchildren = thisDir
            .Children.AsEnumerable()
            .Select(x =>
            {
                if (x is File file)
                {
                    return new File
                        {
                            Path = file.FormatedPath,
                            MTime = file.MTime,
                            NextOp = IsResetNextOpType ? optype : file.NextOp
                        } as AFileOrDir;
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
        ndir.Children = nchildren;

        return ndir;
    }

    /// <summary>
    /// 改变根路径位置
    /// </summary>
    /// <param name="thisDir"></param>
    /// <param name="oldPath"></param>
    /// <param name="newPath"></param>
    /// <returns></returns>
    public static Dir ResetRootPath(this Dir thisDir, string oldPath, string newPath)
    {
        thisDir.FormatedPath = thisDir.FormatedPath.Replace(oldPath, newPath);
        thisDir.Children.ForEach(e =>
        {
            if (e is File file)
            {
                file.FormatedPath = file.FormatedPath.Replace(oldPath, newPath);
            }
            else if (e is Dir dir)
            {
                dir.ResetRootPath(oldPath, newPath);
            }
        });
        return thisDir;
    }

    /// <summary>
    /// 文件操作权限检查（废弃）
    /// </summary>
    /// <param name="thisDir"></param>
    public static void AccessCheck(this Dir thisDir)
    {
        //不是核心关注点，下面实现有bug。不校验所有文件夹权限，创建时会抛出错误，此时手动处理吧。
        return;
        //this.Children.ForEach(e =>
        //{
        //    if (e is File file)
        //    {
        //        if (file.NextOp == null) { }
        //        else if (file.NextOp == NextOpType.Add)
        //        {
        //            if (
        //                !AccessWrapper.CheckDirAccess(
        //                    Path.GetDirectoryName(file.FormatedPath)
        //                        ?? throw new DirectoryNotFoundException(
        //                            $"{file.FormatedPath} 此父路径不存在"
        //                        ),
        //                    [DirAcess.CreateFiles]
        //                )
        //            )
        //            {
        //                throw new UnauthorizedAccessException($"{file.FormatedPath} 无权限创建文件");
        //            }
        //        }
        //        else if (file.NextOp == NextOpType.Modify)
        //        {
        //            if (
        //                !(
        //                    AccessWrapper.CheckFileAccess(file.FormatedPath, [FileAccess.Delete])
        //                    && AccessWrapper.CheckDirAccess(
        //                        Path.GetDirectoryName(file.FormatedPath)
        //                            ?? throw new DirectoryNotFoundException(
        //                                $"{file.FormatedPath} 此父路径不存在"
        //                            ),
        //                        [DirAcess.CreateFiles]
        //                    )
        //                )
        //            )
        //            {
        //                throw new UnauthorizedAccessException(
        //                    $"{file.FormatedPath} 无权限删除源文件或者创建新文件"
        //                );
        //            }
        //        }
        //        else if (file.NextOp == NextOpType.Del)
        //        {
        //            if (!AccessWrapper.CheckFileAccess(file.FormatedPath, [FileAccess.Delete]))
        //            {
        //                throw new UnauthorizedAccessException($"{file.FormatedPath} 无权限删除源文件");
        //            }
        //        }
        //    }
        //    else if (e is Dir dir)
        //    {
        //        if (dir.NextOp == null) { }
        //        else if (dir.NextOp == NextOpType.Add)
        //        {
        //            if (
        //                !AccessWrapper.CheckDirAccess(
        //                    Path.GetDirectoryName(dir.FormatedPath)
        //                        ?? throw new DirectoryNotFoundException(
        //                            $"{dir.FormatedPath} 此父路径不存在"
        //                        ),
        //                    [DirAcess.CreateDirectories, DirAcess.CreateFiles]
        //                )
        //            )
        //            {
        //                throw new UnauthorizedAccessException($"{dir.FormatedPath} 无权限创建文件夹或者文件");
        //            }
        //        }
        //        else if (dir.NextOp == NextOpType.Del)
        //        {
        //            if (!AccessWrapper.CheckDirAccess(dir.FormatedPath, [DirAcess.Delete]))
        //            {
        //                throw new UnauthorizedAccessException($"{dir.FormatedPath} 无权限删除文件夹");
        //            }
        //            else
        //            {
        //                //校验是否拥有子文件或者文件夹的删除权限，
        //                dir.AccessCheck();
        //            }
        //        }
        //    }
        //});
    }
}
