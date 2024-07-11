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
            using (FileStream fs = System.IO.File.OpenWrite(file.Path))
            {
                byte[] info = Encoding.UTF8.GetBytes($"this is  {file.Path},Now{DateTime.Now}");
                fs.Write(info, 0, info.Length);
            }
            Console.WriteLine($"WriteFileStrageFunc {file.Path}");
            System.IO.File.SetLastWriteTime(file.Path, file.MTime);
            return (true, "");
        };
        Console.WriteLine("FilesSeed Construct");
        // string TestPath = Path.Combine(Directory.GetCurrentDirectory(), "../../..");
        DateTime beforeTime = DateTime.Now.AddSeconds(-99);
        DateTime afterTime = beforeTime.AddSeconds(-20);
        NewDir = new Dir(
            TestPath + "/NewDir",
            [
                new Dir($"{TestPath}/NewDir/0"),
                new Dir(
                    $"{TestPath}/NewDir/1",
                    [new Common.File($"{TestPath}/NewDir/1/1.txt", beforeTime)]
                ),
                new Dir(
                    $"{TestPath}/NewDir/2",
                    [
                        new Common.File($"{TestPath}/NewDir/2/2.txt", beforeTime),
                        new Dir(
                            $"{TestPath}/NewDir/2/2_1",
                            [
                                new Common.File($"{TestPath}/NewDir/2/2_1/1.txt", beforeTime),
                                new Common.File($"{TestPath}/NewDir/2/2_1/2.txt", beforeTime),
                            ]
                        ),
                        new Dir(
                            $"{TestPath}/NewDir/2/2_2",
                            [
                                new Common.File($"{TestPath}/NewDir/2/2_2/1.txt", beforeTime),
                                new Common.File($"{TestPath}/NewDir/2/2_2/2.txt", beforeTime),
                                new Dir(
                                    $"{TestPath}/NewDir/2/2_2/2_3",
                                    [
                                        new Common.File(
                                            $"{TestPath}/NewDir/2/2_2/2_3/1.txt",
                                            beforeTime
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
                    [new Common.File($"{TestPath}/OldDir/1/2_D.txt", beforeTime, NextOpType.Del),]
                ),
                new Dir(
                    $"{TestPath}/OldDir/2",
                    [
                        // 将要添加
                        new Common.File($"{TestPath}/OldDir/2/2.txt", beforeTime, NextOpType.Add),
                        new Dir(
                            $"{TestPath}/OldDir/2/2_1",
                            [
                                new Common.File(
                                    $"{TestPath}/OldDir/2/2_1/2.txt",
                                    beforeTime,
                                    NextOpType.Modify
                                ),
                            ]
                        ),
                        new Dir(
                            $"{TestPath}/OldDir/2/2_2_M",
                            [
                                new Common.File(
                                    $"{TestPath}/OldDir/2/2_2/1.txt",
                                    afterTime,
                                    NextOpType.Del
                                ),
                                new Common.File(
                                    $"{TestPath}/OldDir/2/2_2/2.txt",
                                    afterTime,
                                    NextOpType.Del
                                ),
                                new Dir(
                                    $"{TestPath}/OldDir/2/2_2/2_3",
                                    [
                                        new Common.File(
                                            $"{TestPath}/OldDir/2/2_2/2_3/1.txt",
                                            afterTime,
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
                                    beforeTime,
                                    NextOpType.Add
                                ),
                                new Common.File(
                                    $"{TestPath}/OldDir/2/2_2/2.txt",
                                    afterTime,
                                    NextOpType.Add
                                ),
                                new Dir(
                                    $"{TestPath}/OldDir/2/2_2/2_3",
                                    [
                                        new Common.File(
                                            $"{TestPath}/OldDir/2/2_2/2_3/1.txt",
                                            afterTime,
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
                        new Common.File($"{TestPath}/OldDir/1/1.txt", beforeTime),
                        //将要删除
                        new Common.File($"{TestPath}/OldDir/1/2_D.txt", beforeTime),
                    ]
                ),
                new Dir(
                    $"{TestPath}/OldDir/2",
                    [
                        // 将要添加
                        // new Common.File($"{TestPath}/OldDir/2/2.txt", beforeTime),
                        new Dir(
                            $"{TestPath}/OldDir/2/2_1",
                            [
                                new Common.File($"{TestPath}/OldDir/2/2_1/1.txt", beforeTime),
                                new Common.File($"{TestPath}/OldDir/2/2_1/2.txt", afterTime),
                            ]
                        ),
                        new Dir(
                            $"{TestPath}/OldDir/2/2_2_M",
                            [
                                new Common.File($"{TestPath}/OldDir/2/2_2/1.txt", afterTime),
                                new Common.File($"{TestPath}/OldDir/2/2_2/2.txt", afterTime),
                                new Dir(
                                    $"{TestPath}/OldDir/2/2_2/2_3",
                                    [
                                        new Common.File(
                                            $"{TestPath}/OldDir/2/2_2/2_3/1.txt",
                                            afterTime
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
        Directory.Delete(NewDir.Path, true);
        Console.WriteLine("FilesSeed Dispose");
        GC.SuppressFinalize(this);
    }
    // ~FilesSeed()
    // {
    //      Console.WriteLine("FilesSeed ~");
    // }
}
