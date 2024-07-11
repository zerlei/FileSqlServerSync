using System.Text;
using Common;

namespace ServerTest;

public class FilesSeed : IDisposable
{
    public FilesSeed()
    {
        Dir.WriteFileStrageFunc = (Common.File file) =>
        {
            //创建或者不创建直接打开文件
            using (FileStream fs = System.IO.File.OpenWrite(file.FormatedPath))
            {
                byte[] info = Encoding.UTF8.GetBytes($"this is  {file.FormatedPath},Now{DateTime.Now}");
                fs.Write(info, 0, info.Length);
            }
            Console.WriteLine($"WriteFileStrageFunc {file.FormatedPath}");
            System.IO.File.SetLastWriteTime(file.FormatedPath, file.MTime);
            return (true, "");
        };
        Console.WriteLine("FilesSeed Construct");
        // string TestPath = Path.Combine(Directory.GetCurrentDirectory(), "../../..");
        DateTime NewTime = DateTime.Now.AddSeconds(-99);
        DateTime OldTime = NewTime.AddSeconds(-20);
        NewDir = new Dir(
            TestPath + "/NewDir",
            [
                new Dir($"{TestPath}/NewDir/0"),
                new Dir(
                    $"{TestPath}/NewDir/1",
                    [new Common.File($"{TestPath}/NewDir/1/1.txt", NewTime)]
                ),
                new Dir(
                    $"{TestPath}/NewDir/2",
                    [
                        new Common.File($"{TestPath}/NewDir/2/2.txt", NewTime),
                        new Dir(
                            $"{TestPath}/NewDir/2/2_1",
                            [
                                new Common.File($"{TestPath}/NewDir/2/2_1/1.txt", NewTime),
                                new Common.File($"{TestPath}/NewDir/2/2_1/2.txt", NewTime),
                            ]
                        ),
                        new Dir(
                            $"{TestPath}/NewDir/2/2_2",
                            [
                                new Common.File($"{TestPath}/NewDir/2/2_2/1.txt", NewTime),
                                new Common.File($"{TestPath}/NewDir/2/2_2/2.txt", NewTime),
                                new Dir(
                                    $"{TestPath}/NewDir/2/2_2/2_3",
                                    [
                                        new Common.File(
                                            $"{TestPath}/NewDir/2/2_2/2_3/1.txt",
                                            NewTime
                                        ),
                                    ]
                                ),
                            ]
                        )
                    ]
                ),
            ]
        );
        DiffDir = new Dir(
            $"{TestPath}/OldDir",
            [
                new Dir(
                    $"{TestPath}/OldDir/1",
                    [new Common.File($"{TestPath}/OldDir/1/2_D.txt", NewTime, NextOpType.Del),]
                ),
                new Dir(
                    $"{TestPath}/OldDir/2",
                    [
                        // 将要添加
                        new Common.File($"{TestPath}/OldDir/2/2.txt", NewTime, NextOpType.Add),
                        new Dir(
                            $"{TestPath}/OldDir/2/2_1",
                            [
                                new Common.File(
                                    $"{TestPath}/OldDir/2/2_1/2.txt",
                                    NewTime,
                                    NextOpType.Modify
                                ),
                            ]
                        ),
                        new Dir(
                            $"{TestPath}/OldDir/2/2_2_M",
                            [
                                new Common.File(
                                    $"{TestPath}/OldDir/2/2_2/1.txt",
                                    OldTime,
                                    NextOpType.Del
                                ),
                                new Common.File(
                                    $"{TestPath}/OldDir/2/2_2/2.txt",
                                    OldTime,
                                    NextOpType.Del
                                ),
                                new Dir(
                                    $"{TestPath}/OldDir/2/2_2/2_3",
                                    [
                                        new Common.File(
                                            $"{TestPath}/OldDir/2/2_2/2_3/1.txt",
                                            OldTime,
                                            NextOpType.Del
                                        ),
                                    ],
                                    NextOpType.Del
                                ),
                            ],
                            NextOpType.Del
                        ),
                        new Dir(
                            $"{TestPath}/OldDir/2/2_2",
                            [
                                new Common.File(
                                    $"{TestPath}/OldDir/2/2_2/1.txt",
                                    NewTime,
                                    NextOpType.Add
                                ),
                                new Common.File(
                                    $"{TestPath}/OldDir/2/2_2/2.txt",
                                    NewTime,
                                    NextOpType.Add
                                ),
                                new Dir(
                                    $"{TestPath}/OldDir/2/2_2/2_3",
                                    [
                                        new Common.File(
                                            $"{TestPath}/OldDir/2/2_2/2_3/1.txt",
                                            NewTime,
                                            NextOpType.Add
                                        ),
                                    ],
                                    NextOpType.Add
                                ),
                            ],
                            NextOpType.Add
                        )
                    ]
                ),
            ]
        );
        OldDir = new Dir(
            $"{TestPath}/OldDir",
            [
                new Dir($"{TestPath}/OldDir/0"),
                new Dir(
                    $"{TestPath}/OldDir/1",
                    [
                        //不做修改
                        new Common.File($"{TestPath}/OldDir/1/1.txt", NewTime),
                        //将要删除
                        new Common.File($"{TestPath}/OldDir/1/2_D.txt", NewTime),
                    ]
                ),
                new Dir(
                    $"{TestPath}/OldDir/2",
                    [
                        new Dir(
                            $"{TestPath}/OldDir/2/2_1",
                            [
                                new Common.File($"{TestPath}/OldDir/2/2_1/1.txt", NewTime),
                                new Common.File($"{TestPath}/OldDir/2/2_1/2.txt", OldTime),
                            ]
                        ),
                        new Dir(
                            $"{TestPath}/OldDir/2/2_2_M",
                            [
                                new Common.File($"{TestPath}/OldDir/2/2_2/1.txt", OldTime),
                                new Common.File($"{TestPath}/OldDir/2/2_2/2.txt", OldTime),
                                new Dir(
                                    $"{TestPath}/OldDir/2/2_2/2_3",
                                    [
                                        new Common.File(
                                            $"{TestPath}/OldDir/2/2_2/2_3/1.txt",
                                            OldTime
                                        ),
                                    ]
                                ),
                            ]
                        )
                    ]
                ),
            ]
        );
    }

    private readonly string TestPath = Path.Combine(Directory.GetCurrentDirectory(), "../../..");
    public Dir NewDir;
    public Dir OldDir;

    public Dir DiffDir;

    public void Dispose()
    {
        Directory.Delete(NewDir.FormatedPath, true);
        Console.WriteLine("FilesSeed Dispose");
        GC.SuppressFinalize(this);
    }
    // ~FilesSeed()
    // {
    //      Console.WriteLine("FilesSeed ~");
    // }
}
