using Common;
using Newtonsoft.Json;
using XUnit.Project.Attributes;

namespace ServerTest;

/// <summary>
/// xUnit将会对每个测试方法创建一个测试上下文，IClassFixture可以用来创建类中共享测试上下文，
///
/// XUnit 的测试方法不是按照顺序执行，所以注意对象状态
///
/// 一般单元测试，每个测试函数应当是独立的，不让它们按照顺序执行，在一般情况下是最好的做法，参考
/// https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices
/// 目前涉及到一些文件的同步，所以按照顺序执行相对较好，这使用了xUnit的方法使它们按照顺序执行
/// </summary>
///
[TestCaseOrderer(
    ordererTypeName: "XUnit.Project.Orderers.PriorityOrderer",
    ordererAssemblyName: "ServerTest"
)]
public class DirFileOpTest : IDisposable
{
    private readonly FilesSeed filesSeed = new();

    public void Dispose()
    {
        filesSeed.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 测试文件目录写入和提取
    /// </summary>
    [Fact, TestPriority(0)]
    public void FileDirWriteExtract()
    {
        filesSeed.NewDir.WriteByThisInfo(filesSeed.fileDirOp);
        filesSeed.OldDir.WriteByThisInfo(filesSeed.fileDirOp);
        Dir nnd = new() { Path = filesSeed.NewDir.FormatedPath, Children = [] };
        nnd.ExtractInfo();
        Assert.True(nnd.IsEqual(filesSeed.NewDir), "新文件提取文件夹的信息与写入信息不一致！");
        Dir nod = new() { Path = filesSeed.OldDir.FormatedPath, Children = [] };
        nod.ExtractInfo();
        Assert.True(nod.IsEqual(filesSeed.OldDir), "旧提取文件夹的信息与写入信息不一致！");
    }

    /// <summary>
    /// 测试文件差异比较
    /// </summary>
    [Fact, TestPriority(1)]
    public void FileDirDiff()
    {
        var cDDir = filesSeed.NewDir.Diff(filesSeed.OldDir);
        // Console.WriteLine("################################");
        // Console.WriteLine(cDDir.Children.Count);
        //Assert.True(IsSuccess);

        // var str = JsonConvert.SerializeObject(cDDir);
        Assert.True(filesSeed.DiffDir.IsEqual(cDDir), "文件对比结果错误！");
    }

    /// <summary>
    /// 测试同步是否成功
    /// </summary>
    [Fact, TestPriority(2)]
    public void SyncFileDir()
    {
        filesSeed.OldDir.WriteByThisInfo(filesSeed.fileDirOp);
        filesSeed.OldDir.Combine(filesSeed.fileDirOp, filesSeed.DiffDir, false, true);
        Dir oldSync = new() { Path = filesSeed.OldDir.FormatedPath, Children = [] };
        oldSync.ExtractInfo();
        oldSync.ResetRootPath(filesSeed.OldDir.FormatedPath, filesSeed.NewDir.FormatedPath);
        Assert.True(oldSync.IsEqual(filesSeed.NewDir), "文件夹同步后信息保持不一致！");
    }

    /// <summary>
    /// 测试文件合并
    /// </summary>
    [Fact, TestPriority(3)]
    public void DirsCombine()
    {
        filesSeed.OldDir.Combine(null, filesSeed.DiffDir);
        //Assert.False(filesSeed.NewDir.IsEqual(filesSeed.OldDir));
        filesSeed.OldDir.ResetRootPath("OldDir", "NewDir");
        // Console.WriteLine(filesSeed.OldDir.Path);
        Assert.True(filesSeed.OldDir.IsEqual(filesSeed.NewDir), "合并结果不一致！");
    }

    // [Fact]
    // public void Tt()
    // {
    //     filesSeed.NewDir.WriteByThisInfo(filesSeed.fileDirOp);
    //     var c = new FileDirOpForPack(filesSeed.NewDir.FormatedPath, filesSeed.TestPath + "/");
    //     c.FinallyCompress();

    //     var d = new FileDirOpForUnpack(
    //         filesSeed.TestPath + "/",
    //         filesSeed.TestPath + "/",
    //         c.SyncId
    //     );
    //     d.FirstUnComparess();
    // }
}
