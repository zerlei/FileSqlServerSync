config = {
  Name: "Test",
  RemoteUrl: "D:/FileSyncTest/dtemp",
  RemotePwd: "t123",
  IsDeployDb: false,
  IsDeployProject: false,
  LocalProjectAbsolutePath:
    "D:/git/HMES-H7-HNFY/HMES-H7-HNFYMF/HMES-H7-HNFYMF.WEB",
  LocalRootPath: "D:/FileSyncTest/src",

  RemoteRootPath: "D:/FileSyncTest/dst",
  SrcDb: {
    ServerName: "172.16.12.2",
    DatebaseName: "HMES_H7_HNFYMF",
    User: "hmes-h7",
    Password: "Hmes-h7666",
    TrustServerCertificate: "True",
    SyncTablesData: [
      "dbo.sys_Button",
      "dbo.sys_Menu",
      "dbo.sys_Module",
      "dbo.sys_Page",
    ],
  },
  DstDb: {
    ServerName: "127.0.0.1",
    DatebaseName: "HMES_H7_HNFYMF",
    User: "sa",
    Password: "0",
    TrustServerCertificate: "True",
  },
  DirFileConfigs: [
    {
      DirPath: "/bin",
      Excludes: ["/roslyn", "/Views"],
    },
  ],
};
