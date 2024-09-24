using Common;

namespace ServerTest;

public class FilesSeed : IDisposable
{
    public FilesSeed()
    {
        Console.WriteLine("FilesSeed Construct");
        // string TestPath = Path.Combine(Directory.GetCurrentDirectory(), "../../..");
        DateTime NewTime = DateTime.Now.AddSeconds(-99);
        DateTime OldTime = NewTime.AddSeconds(-20);
        NewDir = new Dir
        {
            Path = TestPath + "/NewDir",
            Children =
            [
                new Dir { Path = $"{TestPath}/NewDir/0", Children = [] },
                new Dir
                {
                    Path = $"{TestPath}/NewDir/1",
                    Children =
                    [
                        new Common.File { Path = $"{TestPath}/NewDir/1/1.txt", MTime = NewTime }
                    ]
                },
                new Dir
                {
                    Path = $"{TestPath}/NewDir/2",
                    Children =
                    [
                        new Common.File { Path = $"{TestPath}/NewDir/2/2.txt", MTime = NewTime },
                        new Dir
                        {
                            Path = $"{TestPath}/NewDir/2/2_1",
                            Children =
                            [
                                new Common.File
                                {
                                    Path = $"{TestPath}/NewDir/2/2_1/1.txt",
                                    MTime = NewTime
                                },
                                new Common.File
                                {
                                    Path = $"{TestPath}/NewDir/2/2_1/2.txt",
                                    MTime = NewTime
                                },
                            ]
                        },
                        new Dir
                        {
                            Path = $"{TestPath}/NewDir/2/2_2",
                            Children =
                            [
                                new Common.File
                                {
                                    Path = $"{TestPath}/NewDir/2/2_2/1.txt",
                                    MTime = NewTime
                                },
                                new Common.File
                                {
                                    Path = $"{TestPath}/NewDir/2/2_2/2.txt",
                                    MTime = NewTime
                                },
                                new Dir
                                {
                                    Path = $"{TestPath}/NewDir/2/2_2/2_3",
                                    Children =
                                    [
                                        new Common.File
                                        {
                                            Path = $"{TestPath}/NewDir/2/2_2/2_3/1.txt",
                                            MTime = NewTime
                                        },
                                    ]
                                },
                            ]
                        }
                    ]
                },
            ]
        };
        DiffDir = new Dir
        {
            Path = $"{TestPath}/OldDir",
            Children =
            [
                new Dir
                {
                    Path = $"{TestPath}/OldDir/1",
                    Children =
                    [
                        new Common.File
                        {
                            Path = $"{TestPath}/OldDir/1/2_D.txt",
                            MTime = NewTime,
                            NextOp = NextOpType.Del
                        },
                    ]
                },
                new Dir
                {
                    Path = $"{TestPath}/OldDir/2",
                    Children =
                    [
                        // 将要添加
                        new Common.File
                        {
                            Path = $"{TestPath}/OldDir/2/2.txt",
                            MTime = NewTime,
                            NextOp = NextOpType.Add
                        },
                        new Dir
                        {
                            Path = $"{TestPath}/OldDir/2/2_1",
                            Children =
                            [
                                new Common.File
                                {
                                    Path = $"{TestPath}/OldDir/2/2_1/2.txt",
                                    MTime = NewTime,
                                    NextOp = NextOpType.Modify
                                },
                            ]
                        },
                        new Dir
                        {
                            Path = $"{TestPath}/OldDir/2/2_2_M",
                            Children =
                            [
                                new Common.File
                                {
                                    Path = $"{TestPath}/OldDir/2/2_2_M/1.txt",
                                    MTime = OldTime,
                                    NextOp = NextOpType.Del
                                },
                                new Common.File
                                {
                                    Path = $"{TestPath}/OldDir/2/2_2_M/2.txt",
                                    MTime = OldTime,
                                    NextOp = NextOpType.Del
                                },
                                new Dir
                                {
                                    Path = $"{TestPath}/OldDir/2/2_2_M/2_3",
                                    Children =
                                    [
                                        new Common.File
                                        {
                                            Path = $"{TestPath}/OldDir/2/2_2_M/2_3/1.txt",
                                            MTime = OldTime,
                                            NextOp = NextOpType.Del
                                        },
                                    ],
                                    NextOp = NextOpType.Del
                                },
                            ],
                            NextOp = NextOpType.Del
                        },
                        new Dir
                        {
                            Path = $"{TestPath}/OldDir/2/2_2",
                            Children =
                            [
                                new Common.File
                                {
                                    Path = $"{TestPath}/OldDir/2/2_2/1.txt",
                                    MTime = NewTime,
                                    NextOp = NextOpType.Add
                                },
                                new Common.File
                                {
                                    Path = $"{TestPath}/OldDir/2/2_2/2.txt",
                                    MTime = NewTime,
                                    NextOp = NextOpType.Add
                                },
                                new Dir
                                {
                                    Path = $"{TestPath}/OldDir/2/2_2/2_3",
                                    Children =
                                    [
                                        new Common.File
                                        {
                                            Path = $"{TestPath}/OldDir/2/2_2/2_3/1.txt",
                                            MTime = NewTime,
                                            NextOp = NextOpType.Add
                                        },
                                    ],
                                    NextOp = NextOpType.Add
                                },
                            ],
                            NextOp = NextOpType.Add
                        }
                    ]
                },
            ]
        };
        OldDir = new Dir
        {
            Path = $"{TestPath}/OldDir",
            Children =
            [
                new Dir { Path = $"{TestPath}/OldDir/0", Children = [] },
                new Dir
                {
                    Path = $"{TestPath}/OldDir/1",
                    Children =
                    [
                        //不做修改
                        new Common.File { Path = $"{TestPath}/OldDir/1/1.txt", MTime = NewTime },
                        //将要删除
                        new Common.File { Path = $"{TestPath}/OldDir/1/2_D.txt", MTime = NewTime },
                    ]
                },
                new Dir
                {
                    Path = $"{TestPath}/OldDir/2",
                    Children =
                    [
                        new Dir
                        {
                            Path = $"{TestPath}/OldDir/2/2_1",
                            Children =
                            [
                                new Common.File
                                {
                                    Path = $"{TestPath}/OldDir/2/2_1/1.txt",
                                    MTime = NewTime
                                },
                                new Common.File
                                {
                                    Path = $"{TestPath}/OldDir/2/2_1/2.txt",
                                    MTime = OldTime
                                },
                            ]
                        },
                        new Dir
                        {
                            Path = $"{TestPath}/OldDir/2/2_2_M",
                            Children =
                            [
                                new Common.File
                                {
                                    Path = $"{TestPath}/OldDir/2/2_2_M/1.txt",
                                    MTime = OldTime
                                },
                                new Common.File
                                {
                                    Path = $"{TestPath}/OldDir/2/2_2_M/2.txt",
                                    MTime = OldTime
                                },
                                new Dir
                                {
                                    Path = $"{TestPath}/OldDir/2/2_2_M/2_3",
                                    Children =
                                    [
                                        new Common.File
                                        {
                                            Path = $"{TestPath}/OldDir/2/2_2_M/2_3/1.txt",
                                            MTime = OldTime
                                        },
                                    ]
                                },
                            ]
                        }
                    ]
                },
            ]
        };
        fileDirOp = new SimpleFileDirOp();
    }

    public readonly string TestPath = Path.Combine(Directory.GetCurrentDirectory(), "../../..");
    public Dir NewDir;
    public Dir OldDir;
    public Dir DiffDir;
    public FileDirOpStra fileDirOp;

    public void Dispose()
    {
        if (Directory.Exists($"{TestPath}/OldDir"))
        {
            Directory.Delete($"{TestPath}/OldDir", true);
        }
        if (Directory.Exists($"{TestPath}/NewDir"))
        {
            Directory.Delete($"{TestPath}/NewDir", true);
        }
        Console.WriteLine("FilesSeed Dispose");
        GC.SuppressFinalize(this);
    }
    // ~FilesSeed()
    // {
    //      Console.WriteLine("FilesSeed ~");
    // }
}
