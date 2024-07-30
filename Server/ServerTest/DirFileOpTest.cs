using Common;
using Xunit;

/*using Newtonsoft.Json;*/

namespace ServerTest;

public class DirFileOpTest : IDisposable
{
    private readonly FilesSeed filesSeed = new();

    public void Dispose()
    {
        //filesSeed.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 测试文件目录写入和提取
    /// </summary>
    [Fact]
    public void FileDirWriteExtract()
    {
        filesSeed.NewDir.WriteByThisInfo(filesSeed.fileDirOp);
        filesSeed.OldDir.WriteByThisInfo(filesSeed.fileDirOp);
        Dir nnd = new(filesSeed.NewDir.FormatedPath);
        nnd.ExtractInfo();
        Assert.True(nnd.IsEqual(filesSeed.NewDir), "新文件提取文件夹的信息与写入信息不一致！");
        Dir nod = new(filesSeed.OldDir.FormatedPath);
        nod.ExtractInfo();
        Assert.True(nod.IsEqual(filesSeed.OldDir), "旧提取文件夹的信息与写入信息不一致！");
    }

    /// <summary>
    /// 测试文件差异比较
    /// </summary>
    [Fact]
    public void FileDirDiff()
    {
        var cDDir = filesSeed.NewDir.Diff(filesSeed.OldDir);
        // Console.WriteLine("################################");
        // Console.WriteLine(cDDir.Children.Count);
        //Assert.True(IsSuccess);

        /*var str = JsonConvert.SerializeObject(cDDir);*/
        Assert.True(filesSeed.DiffDir.IsEqual(cDDir), "文件对比结果错误！");
    }

    /// <summary>
    /// 测试同步是否成功
    /// </summary>
    [Fact]
    public void SyncFileDir()
    {
        filesSeed.OldDir.WriteByThisInfo(filesSeed.fileDirOp);
        filesSeed.OldDir.CombineJustDirFile(filesSeed.fileDirOp, filesSeed.DiffDir);
        Dir oldSync = new(filesSeed.OldDir.FormatedPath);
        oldSync.ExtractInfo();
        oldSync.ResetRootPath(filesSeed.OldDir.FormatedPath, filesSeed.NewDir.FormatedPath);
        Assert.True(oldSync.IsEqual(filesSeed.NewDir), "文件夹同步后信息保持不一致！");
    }

    /// <summary>
    /// 测试文件合并
    /// </summary>
    [Fact]
    public void DirsCombine()
    {
        filesSeed.OldDir.CombineJustObject(filesSeed.DiffDir);
        //Assert.False(filesSeed.NewDir.IsEqual(filesSeed.OldDir));
        filesSeed.OldDir.ResetRootPath("OldDir", "NewDir");
        // Console.WriteLine(filesSeed.OldDir.Path);
        Assert.True(filesSeed.OldDir.IsEqual(filesSeed.NewDir), "合并结果不一致！");
    }

    [Fact]
    public void Tt()
    {
        filesSeed.NewDir.WriteByThisInfo(filesSeed.fileDirOp);
        var c = new FileDirOpForPack(filesSeed.NewDir.FormatedPath, filesSeed.TestPath + "/");
        c.FinallyCompress();

        var d = new FileDirOpForUnpack(
            filesSeed.TestPath + "/",
            filesSeed.TestPath + "/",
            c.SyncId
        );
        d.FirstUnComparess();
    }
}
